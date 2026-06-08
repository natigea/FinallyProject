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
