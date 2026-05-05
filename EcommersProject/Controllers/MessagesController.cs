using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class MessagesController(IMessageService messages) : Controller
{
    // Inbox
    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var convs = await messages.GetUserConversationsAsync(userId);
        return View(convs);
    }

    // Open conversation
    public async Task<IActionResult> Chat(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetConversationAsync(id);
        if (conv.BuyerId != userId && conv.SellerId != userId) return Forbid();
        await messages.MarkAsReadAsync(id, userId);
        return View(conv);
    }

    // Start new conversation from listing detail page
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid listingId, string text)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetOrCreateConversationAsync(
            new ConversationStartDto(listingId, userId, text));
        return RedirectToAction(nameof(Chat), new { id = conv.Id });
    }

    // Send a message
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid conversationId, string text)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var conv = await messages.GetConversationAsync(conversationId);
        if (conv.BuyerId != userId && conv.SellerId != userId) return Forbid();

        if (!string.IsNullOrWhiteSpace(text))
            await messages.SendMessageAsync(new MessageCreateDto(conversationId, userId, text));

        return RedirectToAction(nameof(Chat), new { id = conversationId });
    }
}
