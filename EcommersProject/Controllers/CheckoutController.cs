using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.BLL.Services;
using EcommersProject.Models;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Stripe;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
public class CheckoutController(
    IListingService listings,
    IPurchaseService purchases,
    INotificationService notifications,
    IConfiguration configuration,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    // ── Stripe: Create Payment Intent (AJAX) ─────────────────
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest req)
    {
        ListingGetDto? listing;
        long amountInCents;

        if (req.Type == "vip")
        {
            listing = await TryGetListing(req.ListingId);
            if (listing is null) return NotFound();
            amountInCents = (long)(PurchaseService.GetVipPrice(req.VipDays) * 100);
        }
        else
        {
            listing = await GetActiveListing(req.ListingId);
            if (listing is null) return NotFound();
            var deliveryFee = Math.Max(Math.Round(listing.Price * 0.03m, 2), 1m);
            amountInCents = (long)((listing.Price + deliveryFee) * 100);
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount   = amountInCents,
            Currency = "usd",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };
        var service = new PaymentIntentService();
        var intent  = await service.CreateAsync(options);
        return Json(new { clientSecret = intent.ClientSecret });
    }

    // ── Delivery ──────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Delivery(Guid listingId)
    {
        var listing = await GetActiveListing(listingId);
        if (listing is null) return NotFound();
        if (!CanBuy(listing)) return BuyError(listing);
        ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];
        return View(new DeliveryViewModel { Listing = listing });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delivery(Guid listingId, DeliveryViewModel vm)
    {
        var listing = await GetActiveListing(listingId);
        if (listing is null) return NotFound();
        vm.Listing = listing;
        ModelState.Remove(nameof(vm.Listing));
        ModelState.Remove(nameof(vm.StripePaymentIntentId));
        if (!ModelState.IsValid)
        {
            ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];
            return View(vm);
        }

        var (cardLast4, error) = await VerifyPaymentIntent(vm.StripePaymentIntentId);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];
            return View(vm);
        }

        var userId  = CurrentUserId();
        var purchase = await purchases.CreateDeliveryAsync(new DeliveryCreateDto(
            userId, listingId, listing.Title, listing.Price,
            vm.DeliveryAddress, vm.DeliveryCity, vm.DeliveryPhone,
            vm.StripePaymentIntentId, cardLast4!));

        await listings.CloseAsync(listingId);

        if (purchase.SellerId.HasValue)
        {
            await notifications.CreateAsync(
                purchase.SellerId.Value,
                title: listing.Title,
                body: listing.Title,
                link: "/Notifications",
                purchaseId: purchase.Id,
                titleKey: "Notif_NewOrder");

            await notifications.CreateAsync(
                userId,
                title: purchase.OrderNumber,
                body: purchase.OrderNumber,
                link: "/Checkout/History",
                titleKey: "Notif_OrderPending");
        }

        return RedirectToAction(nameof(Result), new { id = purchase.Id });
    }

    // ── VIP ──────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Vip(Guid listingId)
    {
        var listing = await TryGetListing(listingId);
        if (listing is null) return NotFound();
        if (!IsOwner(listing))
        {
            TempData["Error"] = localizer["Chk_OwnListingVip"].Value;
            return RedirectToAction("Detail", "Listing", new { id = listingId });
        }
        ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];
        return View(new VipViewModel { Listing = listing });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Vip(Guid listingId, VipViewModel vm)
    {
        var listing = await TryGetListing(listingId);
        if (listing is null) return NotFound();
        vm.Listing = listing;
        ModelState.Remove(nameof(vm.Listing));
        ModelState.Remove(nameof(vm.StripePaymentIntentId));
        if (!ModelState.IsValid)
        {
            ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];
            return View(vm);
        }

        var (cardLast4, error) = await VerifyPaymentIntent(vm.StripePaymentIntentId);
        if (error != null)
        {
            ModelState.AddModelError("", error);
            ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];
            return View(vm);
        }

        var purchase = await purchases.CreateVipAsync(new VipCreateDto(
            CurrentUserId(), listingId, listing.Title, listing.Price,
            vm.VipDays, PurchaseService.GetVipPrice(vm.VipDays),
            vm.StripePaymentIntentId, cardLast4!));

        return RedirectToAction(nameof(Result), new { id = purchase.Id });
    }

    // ── Stripe verify helper ──────────────────────────────────
    private async Task<(string? cardLast4, string? error)> VerifyPaymentIntent(string paymentIntentId)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            return (null, localizer["Chk_PaymentNotCompleted"].Value);
        try
        {
            var svc    = new PaymentIntentService();
            var intent = await svc.GetAsync(paymentIntentId,
                new PaymentIntentGetOptions { Expand = ["payment_method"] });
            if (intent.Status != "succeeded")
                return (null, localizer["Chk_PaymentFailed"].Value);
            var last4 = intent.PaymentMethod?.Card?.Last4 ?? "****";
            return (last4, null);
        }
        catch
        {
            return (null, localizer["Chk_PaymentVerifyError"].Value);
        }
    }

    // ── Seller: approve / reject delivery orders ──────────────
    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> ApproveOrder(Guid id)
    {
        var order = await purchases.GetByIdAsync(id);
        if (order == null || order.SellerId != CurrentUserId()) return Forbid();
        await purchases.ApproveAsync(id);
        return Ok(new { ok = true });
    }

    [HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> RejectOrder(Guid id)
    {
        var order = await purchases.GetByIdAsync(id);
        if (order == null || order.SellerId != CurrentUserId()) return Forbid();
        await purchases.RejectAsync(id);
        return Ok(new { ok = true });
    }

    // ── Result & History ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Result(Guid id)
    {
        var purchase = await purchases.GetByIdAsync(id);
        if (purchase is null) return NotFound();
        return View(purchase);
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var list = await purchases.GetByUserAsync(CurrentUserId());
        return View(list);
    }

    // ── Helpers ──────────────────────────────────────────────
    private async Task<ListingGetDto?> TryGetListing(Guid id)
    {
        try { return await listings.GetByIdAsync(id); }
        catch { return null; }
    }

    private async Task<ListingGetDto?> GetActiveListing(Guid id)
    {
        var l = await TryGetListing(id);
        return l?.Status == "Active" ? l : null;
    }

    private bool IsOwner(ListingGetDto l) => l.UserId == CurrentUserId();
    private bool CanBuy(ListingGetDto l) => !IsOwner(l);
    private IActionResult BuyError(ListingGetDto l)
    {
        TempData["Error"] = localizer["Chk_OwnListingOrder"].Value;
        return RedirectToAction("Detail", "Listing", new { id = l.Id });
    }
    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
