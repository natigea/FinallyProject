using EcommersProject.DAL.Context;
using EcommersProject.Resources;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Text;
using System.Text.Json;

namespace EcommersProject.Services;

public class FcmService(
    AppDbContext db,
    IHttpClientFactory http,
    IConfiguration cfg,
    IWebHostEnvironment env,
    IStringLocalizer<SharedResource> localizer)
{
    private static readonly JsonSerializerOptions _j = new(JsonSerializerDefaults.Web);
    private const string FcmScope = "https://www.googleapis.com/auth/firebase.messaging";

    // Access token cache — shared across all scoped instances (token lives ~1 hour).
    private static string?        _cachedToken;
    private static DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string FcmEndpoint =>
        $"https://fcm.googleapis.com/v1/projects/{cfg["Firebase:ProjectId"]}/messages:send";

    private string ServiceAccountPath =>
        Path.IsPathRooted(cfg["Firebase:ServiceAccountPath"] ?? "serviceaccount.json")
            ? cfg["Firebase:ServiceAccountPath"]!
            : Path.Combine(env.ContentRootPath, cfg["Firebase:ServiceAccountPath"] ?? "serviceaccount.json");

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task SendMessageAsync(
        Guid recipientId, string senderName, string messageText, Guid conversationId)
    {
        var fcmToken = await GetFcmTokenAsync(recipientId);
        if (fcmToken == null) return;

        var body = messageText.Length > 100 ? messageText[..100] + "…" : messageText;
        await PostAsync(fcmToken,
            title: senderName,
            body:  body,
            url:   $"/Messages/Chat/{conversationId}",
            data:  new Dictionary<string, string>
            {
                ["type"]           = "message",
                ["conversationId"] = conversationId.ToString()
            });
    }

    public async Task SendCallAsync(
        Guid recipientId, string callerName, Guid conversationId)
    {
        var fcmToken = await GetFcmTokenAsync(recipientId);
        if (fcmToken == null) return;

        await PostAsync(fcmToken,
            title: string.Format(localizer["Fcm_Calling"], callerName),
            body:  localizer["Fcm_TapToAnswer"],
            url:   $"/Messages/Chat/{conversationId}",
            data:  new Dictionary<string, string>
            {
                ["type"]           = "call",
                ["conversationId"] = conversationId.ToString(),
                ["callerName"]     = callerName
            });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<string?> GetFcmTokenAsync(Guid userId)
    {
        return await db.Users
            .Where(u => u.Id == userId && !u.IsDeleted && u.FcmToken != null)
            .Select(u => u.FcmToken)
            .FirstOrDefaultAsync();
    }

    private async Task PostAsync(
        string fcmToken, string title, string body, string url,
        Dictionary<string, string>? data = null)
    {
        var accessToken = await GetAccessTokenAsync();
        if (accessToken == null) return;

        try
        {
            using var client = http.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // FCM V1 message envelope
            var message = new
            {
                message = new
                {
                    token = fcmToken,

                    // Standard cross-platform notification
                    notification = new { title, body },

                    // Custom data payload (available in service worker + app)
                    data = (data ?? []).Concat(new Dictionary<string, string> { ["url"] = url })
                                       .ToDictionary(kv => kv.Key, kv => kv.Value),

                    // Web-specific options
                    webpush = new
                    {
                        notification = new
                        {
                            icon  = "/img/favicon.svg",
                            badge = "/img/favicon.svg",
                            vibrate = data?.GetValueOrDefault("type") == "call"
                                ? new[] { 400, 150, 400, 150, 400 }
                                : new[] { 200 }
                        },
                        fcm_options = new { link = url }
                    }
                }
            };

            var json = JsonSerializer.Serialize(message, _j);
            var resp = await client.PostAsync(
                FcmEndpoint,
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            {
                // Token may be stale — clear it from DB on 404/410
                var status = (int)resp.StatusCode;
                if (status is 404 or 410)
                    await ClearStaleTokenAsync(fcmToken);
            }
        }
        catch { /* FCM push is best-effort */ }
    }

    // Get a cached OAuth2 access token, refreshing when within 5 minutes of expiry.
    private async Task<string?> GetAccessTokenAsync()
    {
        if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiry - TimeSpan.FromMinutes(5))
            return _cachedToken;

        await _tokenLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiry - TimeSpan.FromMinutes(5))
                return _cachedToken;

            var path = ServiceAccountPath;
            if (!File.Exists(path)) return null;

            await using var stream = File.OpenRead(path);
            var credential = GoogleCredential
                .FromStream(stream)
                .CreateScoped(FcmScope);

            // GoogleCredential implements ITokenAccess — use that interface for the token
            _cachedToken = await ((ITokenAccess)credential).GetAccessTokenForRequestAsync();
            _tokenExpiry = DateTimeOffset.UtcNow.AddHours(1);
            return _cachedToken;
        }
        catch { return null; }
        finally { _tokenLock.Release(); }
    }

    // Remove stale FCM token from DB when Firebase says the token is invalid/expired.
    private async Task ClearStaleTokenAsync(string staleToken)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.FcmToken == staleToken);
            if (user == null) return;
            user.FcmToken = null;
            await db.SaveChangesAsync();
        }
        catch { }
    }
}
