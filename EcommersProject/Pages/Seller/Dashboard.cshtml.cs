using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Seller;

[Authorize]
public class DashboardModel(IListingService listingService) : PageModel
{
    public IReadOnlyList<ListingGetDto> MyListings { get; private set; } = [];
    public int ActiveCount { get; private set; }
    public int ClosedCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return;

        MyListings = await listingService.GetByUserAsync(userId, cancellationToken);
        ActiveCount = MyListings.Count(l => l.Status == "Active");
        ClosedCount = MyListings.Count(l => l.Status != "Active");
    }
}
