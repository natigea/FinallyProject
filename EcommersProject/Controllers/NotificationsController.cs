using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
public class NotificationsController(
    INotificationService notifications,
    IPurchaseService purchases) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId();
        var list = await notifications.GetByUserAsync(userId);
        await notifications.MarkAllReadAsync(userId);
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid purchaseId)
    {
        var purchase = await purchases.GetByIdAsync(purchaseId);
        if (purchase is null || purchase.SellerId != CurrentUserId())
            return Forbid();

        await purchases.ApproveAsync(purchaseId);
        await notifications.ClearPurchaseIdAsync(purchaseId);

        await notifications.CreateAsync(
            purchase.UserId,
            "✅ Заказ подтверждён",
            $"Продавец подтвердил ваш заказ «{purchase.ListingTitle}». Ожидайте доставку.",
            "/Checkout/History");

        TempData["Success"] = "Заказ подтверждён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid purchaseId)
    {
        var purchase = await purchases.GetByIdAsync(purchaseId);
        if (purchase is null || purchase.SellerId != CurrentUserId())
            return Forbid();

        await purchases.RejectAsync(purchaseId);
        await notifications.ClearPurchaseIdAsync(purchaseId);

        await notifications.CreateAsync(
            purchase.UserId,
            "❌ Заказ отклонён",
            $"К сожалению, продавец отклонил ваш заказ «{purchase.ListingTitle}».",
            "/Checkout/History");

        TempData["Info"] = "Заказ отклонён.";
        return RedirectToAction(nameof(Index));
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
