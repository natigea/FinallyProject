using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class CheckoutController : Controller
{
    private readonly ICartService _cart;
    private readonly IAddressService _addresses;
    private readonly IOrderService _orders;
    private readonly ICouponService _coupons;

    public CheckoutController(ICartService cart, IAddressService addresses,
        IOrderService orders, ICouponService coupons)
    {
        _cart = cart;
        _addresses = addresses;
        _orders = orders;
        _coupons = coupons;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index(string? couponCode, string? couponMsg, Guid? couponId)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });

        var userId = GetUserId();
        var cart = await _cart.GetByUserAsync(userId);

        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        var addresses = await _addresses.GetByUserAsync(userId);

        decimal discount = 0;
        if (couponId.HasValue && couponCode != null)
        {
            var coupon = await _coupons.GetByCodeAsync(couponCode);
            if (coupon != null)
            {
                var subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
                discount = coupon.DiscountAmount > 0
                    ? coupon.DiscountAmount
                    : subtotal * coupon.DiscountPercent / 100;
            }
        }

        var vm = new CheckoutViewModel
        {
            Cart = cart,
            Addresses = addresses,
            SelectedAddressId = addresses.FirstOrDefault(a => a.IsDefault)?.Id,
            Discount = discount,
            CouponCode = couponCode,
            CouponMessage = couponMsg,
            CouponId = couponId
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string couponCode)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        var coupon = await _coupons.GetByCodeAsync(couponCode);
        if (coupon == null || coupon.ValidTo < DateTimeOffset.UtcNow)
        {
            return RedirectToAction(nameof(Index), new
            {
                couponMsg = "Купон недействителен или истёк."
            });
        }

        return RedirectToAction(nameof(Index), new
        {
            couponCode = coupon.Code,
            couponMsg = "Купон применён!",
            couponId = coupon.Id
        });
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(Guid? addressId, Guid? couponId,
        string? newLine1, string? newCity, string? newState, string? newPostal, string? newCountry)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        var userId = GetUserId();
        var cart = await _cart.GetByUserAsync(userId);

        if (cart == null || !cart.Items.Any())
            return RedirectToAction("Index", "Cart");

        Guid? shippingAddressId = addressId;
        if (shippingAddressId == null && !string.IsNullOrWhiteSpace(newLine1))
        {
            var newAddr = await _addresses.CreateAsync(new AddressCreateDto(
                userId, newLine1!, "", newCity ?? "", newState ?? "",
                newPostal ?? "", newCountry ?? "AZ", false));
            shippingAddressId = newAddr.Id;
        }

        var orderItems = cart.Items.Select(i => new OrderItemCreateDto(i.ProductId, i.Quantity)).ToList();
        var order = await _orders.CreateAsync(new OrderCreateDto(userId, shippingAddressId, couponId, orderItems));

        await _cart.ClearCartAsync(cart.Id);

        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(Guid id)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        try
        {
            var order = await _orders.GetByIdAsync(id);
            return View(new OrderConfirmationViewModel { Order = order });
        }
        catch
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
