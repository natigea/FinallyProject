using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class DashboardModel(IStatisticsService statisticsService) : PageModel
{
    public AdminDashboardDto? Stats { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Stats = await statisticsService.GetAdminDashboardAsync(cancellationToken);
    }
}
