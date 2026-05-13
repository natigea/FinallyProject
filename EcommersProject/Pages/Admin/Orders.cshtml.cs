using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OrdersModel(IOrderService orderService) : PageModel
{
    public IReadOnlyList<OrderGetDto> Orders { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public string[] AllStatuses { get; } =
    [
        OrderStatus.Pending, OrderStatus.Paid, OrderStatus.Processing,
        OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled
    ];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var all = await orderService.GetAllAsync(cancellationToken);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(StatusFilter))
            filtered = filtered.Where(o => o.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));

        Orders = filtered.OrderByDescending(o => o.OrderDate).ToList();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken)
    {
        await orderService.UpdateStatusAsync(id, status, cancellationToken);
        TempData["SuccessMessage"] = $"Статус заказа обновлён: {status}";
        return RedirectToPage();
    }
}
