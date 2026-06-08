using EcommersProject.BLL.Interfaces;
using EcommersProject.BLL.Exceptions;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
public class NotificationsController(
    INotificationService notifications,
    IPurchaseService purchases,
    IListingService listings,
    IStringLocalizer<SharedResource> localizer) : Controller
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
            title: purchase.ListingTitle,
            body: purchase.ListingTitle,
            link: "/Checkout/History",
            titleKey: "Notif_OrderApproved");

        TempData["Success"] = localizer["Ntf_OrderConfirmed"].Value;
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

        try { await listings.ReopenAsync(purchase.ListingId); } catch (NotFoundException) { }

        await notifications.CreateAsync(
            purchase.UserId,
            title: purchase.ListingTitle,
            body: purchase.ListingTitle,
            link: "/Checkout/History",
            titleKey: "Notif_OrderRejected");

        TempData["Info"] = localizer["Ntf_OrderRejected"].Value;
        return RedirectToAction(nameof(Index));
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
