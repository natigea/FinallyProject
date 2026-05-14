using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class CartController : Controller
{
    private readonly ICartService _cart;

    public CartController(ICartService cart)
    {
        _cart = cart;
    }

    private Guid? CurrentUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return val != null ? Guid.Parse(val) : null;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Cart" });

        var userId = CurrentUserId()!.Value;
        var cart = await _cart.GetByUserAsync(userId);

        return View(new CartViewModel { Cart = cart });
    }

    [HttpPost]
    public async Task<IActionResult> AddItem(Guid productId, int quantity = 1)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Shop/Detail/" + productId });

        var userId = CurrentUserId()!.Value;
        var cart = await _cart.GetByUserAsync(userId)
            ?? await _cart.CreateAsync(new CartCreateDto(userId));

        await _cart.AddItemAsync(cart.Id, new CartItemCreateDto(cart.Id, productId, quantity));

        TempData["Success"] = "Товар добавлен в корзину.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        await _cart.RemoveItemAsync(itemId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(Guid itemId, int quantity)
    {
        if (quantity < 1) quantity = 1;
        await _cart.UpdateItemQuantityAsync(itemId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        var userId = CurrentUserId()!.Value;
        var cart = await _cart.GetByUserAsync(userId);
        if (cart != null)
            await _cart.ClearCartAsync(cart.Id);

        return RedirectToAction(nameof(Index));
    }
}
