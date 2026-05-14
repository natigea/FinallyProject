using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class FavoritesController : Controller
{
    private readonly IWishlistService _wishlist;

    public FavoritesController(IWishlistService wishlist)
    {
        _wishlist = wishlist;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Favorites" });

        var items = await _wishlist.GetByUserAsync(GetUserId());
        return View(new FavoritesViewModel { Items = items });
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(Guid productId, string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        await _wishlist.ToggleAsync(GetUserId(), productId);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}
