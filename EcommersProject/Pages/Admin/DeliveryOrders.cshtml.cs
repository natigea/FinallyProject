using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class DeliveryOrdersModel(IPurchaseService purchaseService) : PageModel
{
    public IReadOnlyList<PurchaseGetDto> Orders { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Orders = await purchaseService.GetAllDeliveryAsync(cancellationToken);
    }
}
