using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Hubs;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class CallController(
    IHttpClientFactory httpFactory,
    IConfiguration config,
    IMessageService msgService,
    INotificationService notifService,
    FcmService fcm,
    IHubContext<ChatHub> hubContext) : Controller
{
    // In-memory pending calls (ephemeral, single-server)
    private static readonly ConcurrentDictionary<Guid, PendingCall> _pending = new();
    private static readonly JsonSerializerOptions _jOpts = new(JsonSerializerDefaults.Web);

    // GET /Call/Join/{conversationId}
    [HttpGet]
    public async Task<IActionResult> Join(Guid conversationId)
    {
        var userId   = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.Identity?.Name ?? "Пользователь";
        var apiKey   = config["Daily:ApiKey"]!;
        var roomName = "conv-" + conversationId.ToString("N")[..16];

        using var http = httpFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // Get or create room
        string roomUrl;
        var getResp = await http.GetAsync($"https://api.daily.co/v1/rooms/{roomName}");
        if (getResp.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(await getResp.Content.ReadAsStringAsync());
            roomUrl = doc.RootElement.GetProperty("url").GetString()!;
        }
        else
        {
            var expiry  = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
            var payload = JsonSerializer.Serialize(new { name = roomName, properties = new { exp = expiry, enable_chat = false } });
            var created = await http.PostAsync("https://api.daily.co/v1/rooms",
                          new StringContent(payload, Encoding.UTF8, "application/json"));
            if (!created.IsSuccessStatusCode)
                return StatusCode(502, "Daily.co room creation failed");
            using var doc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
            roomUrl = doc.RootElement.GetProperty("url").GetString()!;
        }

        // Meeting token: skip prejoin, audio-only, set name
        var tokenPayload = JsonSerializer.Serialize(new
        {
            properties = new
            {
                room_name         = roomName,
                user_name         = userName,
                enable_prejoin_ui = false,
                start_video_off   = true,
                start_audio_off   = false
            }
        });
        var tokenResp = await http.PostAsync("https://api.daily.co/v1/meeting-tokens",
                        new StringContent(tokenPayload, Encoding.UTF8, "application/json"));

        string? tokenValue = null;
        if (tokenResp.IsSuccessStatusCode)
        {
            using var tDoc = JsonDocument.Parse(await tokenResp.Content.ReadAsStringAsync());
            if (tDoc.RootElement.TryGetProperty("token", out var tok))
                tokenValue = tok.GetString();
        }

        // If another user already has a pending call for this conversation → callee is accepting
        bool isCaller = !_pending.TryGetValue(conversationId, out var existing) || existing.CallerId == userId;

        if (isCaller)
        {
            // Store pending call for the other participant to detect
            _pending[conversationId] = new PendingCall(conversationId, userId, userName, DateTimeOffset.UtcNow);

            // Notify other participant (in-app + FCM push)
            try
            {
                var conv    = await msgService.GetConversationAsync(conversationId);
                var otherId = conv.BuyerId == userId ? conv.SellerId : conv.BuyerId;
                await notifService.CreateAsync(otherId,
                    "📞 Входящий звонок",
                    $"{userName} звонит вам — откройте чат чтобы ответить",
                    $"/Messages/Chat/{conversationId}");
                // FCM push so the phone rings even when the app is in background
                await fcm.SendCallAsync(otherId, userName, conversationId);
                // SignalR real-time push (instant, works while browser is open)
                await hubContext.Clients.Group($"user-{otherId}")
                    .SendAsync("IncomingCall", userName, conversationId.ToString());
            }
            catch { }
        }
        else
        {
            // Callee accepted → clear pending
            _pending.TryRemove(conversationId, out _);
        }

        return Json(new { url = roomUrl, token = tokenValue, isCaller });
    }

    // GET /Call/Poll?conversationId=...
    [HttpGet]
    public IActionResult Poll(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Expire calls older than 60 seconds
        var cutoff = DateTimeOffset.UtcNow.AddSeconds(-60);
        foreach (var key in _pending.Keys.ToList())
            if (_pending.TryGetValue(key, out var v) && v.StartedAt < cutoff)
                _pending.TryRemove(key, out _);

        if (_pending.TryGetValue(conversationId, out var call) && call.CallerId != userId)
            return Json(new { incoming = true, callerName = call.CallerName });

        return Json(new { incoming = false });
    }

    // GET /Call/Decline?conversationId=...
    [HttpGet]
    public async Task<IActionResult> Decline(Guid conversationId)
    {
        if (_pending.TryRemove(conversationId, out var removed))
        {
            // Notify caller that their call was declined
            try
            {
                await hubContext.Clients.Group($"user-{removed.CallerId}")
                    .SendAsync("CallDeclined", conversationId.ToString());
            }
            catch { }
        }
        return Ok();
    }

    // GET /Call/Log?conversationId=...&duration=...
    [HttpGet]
    public async Task<IActionResult> Log(Guid conversationId, int duration)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var mins   = duration / 60;
        var secs   = duration % 60;
        // Always Russian — hardcoded
        var text   = duration >= 5
            ? $"📞 Голосовой звонок · {mins}:{secs:D2}"
            : "📞 Звонок (не состоялся)";
        try { await msgService.SendMessageAsync(new MessageCreateDto(conversationId, userId, text)); }
        catch { }
        return Ok();
    }

    public record PendingCall(Guid ConversationId, Guid CallerId, string CallerName, DateTimeOffset StartedAt);
}
