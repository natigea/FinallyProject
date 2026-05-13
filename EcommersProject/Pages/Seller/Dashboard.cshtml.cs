using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Seller;

[Authorize(Policy = "SellerOrAdmin")]
public class DashboardModel(IStatisticsService statisticsService) : PageModel
{
    public SellerDashboardDto? Stats { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var sellerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(sellerIdStr, out var sellerId))
            Stats = await statisticsService.GetSellerDashboardAsync(sellerId, cancellationToken);
    }
}
