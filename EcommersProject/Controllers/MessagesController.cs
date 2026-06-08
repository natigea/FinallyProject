using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Hubs;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class MessagesController(
    IMessageService messages,
    IHubContext<ChatHub> hub,
    FcmService fcm) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var convs = await messages.GetUserConversationsAsync(userId);
        return View(convs);
    }

    public async Task<IActionResult> Chat(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetConversationAsync(id);
        if (conv.BuyerId != userId && conv.SellerId != userId) return Forbid();
        await messages.MarkAsReadAsync(id, userId);
        return View(conv);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid listingId, string text)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetOrCreateConversationAsync(
            new ConversationStartDto(listingId, userId, text));
        return RedirectToAction(nameof(Chat), new { id = conv.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid conversationId, string text)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetConversationAsync(conversationId);
        if (conv.BuyerId != userId && conv.SellerId != userId) return Forbid();

        if (string.IsNullOrWhiteSpace(text))
            return IsAjax() ? BadRequest() : RedirectToAction(nameof(Chat), new { id = conversationId });

        var msg = await messages.SendMessageAsync(new MessageCreateDto(conversationId, userId, text));
        var senderName = User.Identity?.Name ?? "";
        var recipientId = conv.BuyerId == userId ? conv.SellerId : conv.BuyerId;

        // Real-time delivery to recipient via SignalR (if their browser tab is open)
        await hub.Clients.Group($"user-{recipientId}").SendAsync("ReceiveMessage", new
        {
            conversationId = msg.ConversationId,
            id             = msg.Id,
            text           = msg.Text,
            senderId       = msg.SenderId,
            senderName     = msg.SenderName,
            createdDate    = msg.CreatedDate,
            isRead         = false
        });

        // FCM push for recipient when their browser is closed / backgrounded
        await fcm.SendMessageAsync(recipientId, senderName, text, conversationId);

        if (IsAjax())
            return Json(new
            {
                id          = msg.Id,
                text        = msg.Text,
                senderId    = msg.SenderId,
                senderName  = msg.SenderName,
                createdDate = msg.CreatedDate
            });

        return RedirectToAction(nameof(Chat), new { id = conversationId });
    }

    // POST /Messages/SendMedia
    [HttpPost]
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> SendMedia(Guid conversationId, IFormFile file, string type)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetConversationAsync(conversationId);
        if (conv.BuyerId != userId && conv.SellerId != userId) return Forbid();
        if (file == null || file.Length == 0) return BadRequest("No file");
        if (type is not ("voice" or "photo")) return BadRequest("Invalid type");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
        Directory.CreateDirectory(uploadsDir);

        var ext = type == "voice" ? ".webm" : Path.GetExtension(file.FileName).ToLowerInvariant();
        if (type == "photo" && ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            ext = ".jpg";
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var mediaUrl = $"/uploads/chat/{fileName}";
        var text = $"[{type}]{mediaUrl}";

        var msg = await messages.SendMessageAsync(new MessageCreateDto(conversationId, userId, text));
        var senderName = User.Identity?.Name ?? "";
        var recipientId = conv.BuyerId == userId ? conv.SellerId : conv.BuyerId;

        await hub.Clients.Group($"user-{recipientId}").SendAsync("ReceiveMessage", new
        {
            conversationId = msg.ConversationId,
            id             = msg.Id,
            text           = msg.Text,
            senderId       = msg.SenderId,
            senderName     = msg.SenderName,
            createdDate    = msg.CreatedDate,
            isRead         = false
        });

        var preview = type == "voice" ? "🎤 Голосовое сообщение" : "📷 Фото";
        await fcm.SendMessageAsync(recipientId, senderName, preview, conversationId);

        return Json(new
        {
            id          = msg.Id,
            text        = msg.Text,
            senderId    = msg.SenderId,
            senderName  = msg.SenderName,
            createdDate = msg.CreatedDate
        });
    }

    // POST /Messages/MarkRead?conversationId=...
    // Called from the client when real-time messages arrive on the open chat page.
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MarkRead(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetConversationAsync(conversationId);
        if (conv.BuyerId != userId && conv.SellerId != userId) return Forbid();
        await messages.MarkAsReadAsync(conversationId, userId);
        return Ok();
    }

    private bool IsAjax() =>
        Request.Headers.XRequestedWith == "XMLHttpRequest";
}
