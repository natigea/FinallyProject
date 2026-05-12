using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IStatisticsService statisticsService) : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var data = await statisticsService.GetAdminDashboardAsync(cancellationToken);
        return Ok(data);
    }

    [HttpGet("seller")]
    [Authorize(Policy = "SellerOrAdmin")]
    [ProducesResponseType(typeof(SellerDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSellerDashboard(CancellationToken cancellationToken)
    {
        var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(sellerId, out var id))
            return Unauthorized();

        var data = await statisticsService.GetSellerDashboardAsync(id, cancellationToken);
        return Ok(data);
    }
}
