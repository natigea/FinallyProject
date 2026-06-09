using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;
using EcommersProject.Hubs;
using EcommersProject.Resources;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class MessagesController(
    IMessageService messages,
    IUnitOfWork uow,
    IHubContext<ChatHub> hub,
    FcmService fcm,
    CloudinaryService cloudinary,
    IStringLocalizer<SharedResource> localizer) : Controller
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

    public async Task<IActionResult> Support()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var admins = await uow.Users.FindAsync(u => u.Role == UserRole.Admin);
        var admin = admins.FirstOrDefault();
        if (admin == null || admin.Id == userId)
            return RedirectToAction(nameof(Index));

        var supportListings = await uow.Listings.FindAsync(l => l.Title == "__SUPPORT__" && l.UserId == admin.Id);
        var supportListing = supportListings.FirstOrDefault();
        if (supportListing == null)
            return RedirectToAction(nameof(Index));

        var conv = await messages.GetOrCreateConversationAsync(
            new ConversationStartDto(supportListing.Id, userId, ""));
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

        string? mediaUrl;
        if (type == "voice")
        {
            await using var stream = file.OpenReadStream();
            mediaUrl = await cloudinary.UploadRawAsync(stream, $"{Guid.NewGuid():N}.webm", "chat/voice");
        }
        else
        {
            mediaUrl = await cloudinary.UploadAsync(file, "chat/photo");
        }
        if (mediaUrl == null) return BadRequest("Upload failed");
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

        var preview = type == "voice" ? localizer["Msg_VoiceMessage"].Value : localizer["Msg_Photo"].Value;
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
