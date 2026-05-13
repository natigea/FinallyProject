using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Seller;

[Authorize(Policy = "SellerOrAdmin")]
public class OrdersModel(IOrderService orderService, IProductService productService) : PageModel
{
    public IReadOnlyList<OrderGetDto> Orders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var sellerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sellerIdStr, out var sellerId))
            return;

        var sellerProducts = await productService.GetAllAsync(cancellationToken);
        var sellerProductIds = sellerProducts
            .Where(p => p.SellerId == sellerId)
            .Select(p => p.Id)
            .ToHashSet();

        var allOrders = await orderService.GetAllAsync(cancellationToken);

        Orders = allOrders
            .Where(o => o.Items.Any(i => sellerProductIds.Contains(i.ProductId)))
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }
}
