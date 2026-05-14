using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.ViewComponents;

public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cart;

    public CartBadgeViewComponent(ICartService cart)
    {
        _cart = cart;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int count = 0;
        var userIdClaim = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            var cart = await _cart.GetByUserAsync(userId);
            count = cart?.Items.Sum(i => i.Quantity) ?? 0;
        }
        return View(count);
    }
}
