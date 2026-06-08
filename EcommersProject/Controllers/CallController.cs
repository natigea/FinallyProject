using EcommersProject.BLL.Interfaces;
using EcommersProject.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class CallController(
    IMessageService messages,
    IHubContext<ChatHub> hub,
    IHttpClientFactory httpFactory,
    IConfiguration config,
    ILogger<CallController> logger) : Controller
{
    private record PendingCall(
        Guid ConversationId,
        Guid CallerId,
        string CallerName,
        string RoomUrl,
        DateTime CreatedAt);

    // key = recipientUserId → pending call info
    private static readonly ConcurrentDictionary<Guid, PendingCall> _pending = new();

    [HttpGet("Call/Join/{conversationId}")]
    public async Task<IActionResult> Join(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        logger.LogInformation("[Call] Join: userId={UserId}, conversationId={ConvId}", userId, conversationId);

        if (conversationId == Guid.Empty)
        {
            logger.LogError("[Call] Join REJECTED: conversationId is Guid.Empty");
            return BadRequest(new { error = "conversationId is empty" });
        }

        var conv = await messages.GetConversationAsync(conversationId);
        if (conv.BuyerId != userId && conv.SellerId != userId)
            return Forbid();

        var recipientId = conv.BuyerId == userId ? conv.SellerId : conv.BuyerId;
        var callerName = User.Identity?.Name ?? "?";

        var existingCall = _pending.Values.FirstOrDefault(p =>
            p.ConversationId == conversationId && p.CallerId != userId);
        var isCaller = existingCall == null;

        logger.LogInformation("[Call] Join: caller={CallerName}, recipient={RecipientId}, isCaller={IsCaller}",
            callerName, recipientId, isCaller);

        string roomUrl;
        if (isCaller)
        {
            roomUrl = await CreateDailyRoom(conversationId);
            logger.LogInformation("[Call] Daily room created: {RoomUrl}", roomUrl);

            var pending = new PendingCall(conversationId, userId, callerName, roomUrl, DateTime.UtcNow);
            _pending[recipientId] = pending;
            logger.LogInformation("[Call] Stored pending call: recipientId={RecipientId}, convId={ConvId}",
                recipientId, conversationId);

            await hub.Clients.Group($"user-{recipientId}")
                .SendAsync("IncomingCall", callerName, conversationId.ToString());
            logger.LogInformation("[Call] SignalR IncomingCall sent to group user-{RecipientId}", recipientId);
        }
        else
        {
            roomUrl = existingCall.RoomUrl;
            logger.LogInformation("[Call] Callee joining existing room: {RoomUrl}", roomUrl);
            _pending.TryRemove(userId, out _);
        }

        return Json(new { roomUrl, isCaller, conversationId });
    }

    [HttpGet("Call/PollIncoming")]
    public IActionResult PollIncoming()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (_pending.TryGetValue(userId, out var call))
        {
            if ((DateTime.UtcNow - call.CreatedAt).TotalSeconds > 60)
            {
                _pending.TryRemove(userId, out _);
                logger.LogInformation("[Call] PollIncoming: expired call removed for userId={UserId}", userId);
                return Json(new { incoming = false });
            }

            logger.LogInformation("[Call] PollIncoming: found call for userId={UserId}, convId={ConvId}, caller={Caller}",
                userId, call.ConversationId, call.CallerName);

            return Json(new
            {
                incoming = true,
                callerName = call.CallerName,
                conversationId = call.ConversationId.ToString()
            });
        }

        return Json(new { incoming = false });
    }

    [HttpGet("Call/Poll")]
    public IActionResult Poll(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (conversationId == Guid.Empty)
        {
            logger.LogWarning("[Call] Poll: conversationId is Guid.Empty, userId={UserId}", userId);
            return Json(new { incoming = false });
        }

        if (_pending.TryGetValue(userId, out var call) && call.ConversationId == conversationId)
        {
            if ((DateTime.UtcNow - call.CreatedAt).TotalSeconds > 60)
            {
                _pending.TryRemove(userId, out _);
                return Json(new { incoming = false });
            }

            return Json(new
            {
                incoming = true,
                callerName = call.CallerName,
                conversationId = call.ConversationId.ToString()
            });
        }

        return Json(new { incoming = false });
    }

    [HttpGet("Call/Decline")]
    public IActionResult Decline(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        logger.LogInformation("[Call] Decline: userId={UserId}, convId={ConvId}", userId, conversationId);

        if (_pending.TryRemove(userId, out var call))
        {
            hub.Clients.Group($"user-{call.CallerId}")
                .SendAsync("CallDeclined", call.ConversationId.ToString());
        }

        return Ok();
    }

    [HttpGet("Call/Log")]
    public IActionResult Log(Guid conversationId, int duration)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        logger.LogInformation("[Call] Log: userId={UserId}, convId={ConvId}, duration={Duration}s",
            userId, conversationId, duration);

        // Clean up pending call for recipient
        foreach (var kvp in _pending)
        {
            if (kvp.Value.ConversationId == conversationId)
            {
                _pending.TryRemove(kvp.Key, out _);
                break;
            }
        }

        return Ok();
    }

    private async Task<string> CreateDailyRoom(Guid conversationId)
    {
        var apiKey = config["DailyCo:ApiKey"];
        var client = httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var roomName = $"call-{conversationId:N}-{DateTime.UtcNow.Ticks}";
        var body = JsonSerializer.Serialize(new
        {
            name = roomName,
            properties = new
            {
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                max_participants = 2,
                enable_chat = false,
                start_video_off = true
            }
        });

        var resp = await client.PostAsync(
            "https://api.daily.co/v1/rooms",
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        var json = await resp.Content.ReadAsStringAsync();
        logger.LogInformation("[Call] Daily API response: {Status} {Body}", resp.StatusCode, json);

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Daily.co room creation failed: {resp.StatusCode} - {json}");

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("url").GetString()!;
    }
}
