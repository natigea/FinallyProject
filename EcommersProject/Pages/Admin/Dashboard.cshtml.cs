using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class DashboardModel(
    IListingService listingService,
    IUserService userService,
    ICategoryService categoryService) : PageModel
{
    public int TotalListings    { get; private set; }
    public int ActiveListings   { get; private set; }
    public int PendingListings  { get; private set; }
    public int TotalUsers       { get; private set; }
    public int TotalCategories  { get; private set; }
    public IReadOnlyList<ListingGetDto> RecentListings { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var (items, total) = await listingService.SearchAsync(
            new ListingSearchDto(null, null, null, null, null, "newest", 1, 5), cancellationToken);
        RecentListings = items;

        var allListings = await listingService.SearchAsync(
            new ListingSearchDto(null, null, null, null, null, "newest", 1, 9999), cancellationToken);
        TotalListings  = allListings.Total;
        ActiveListings = allListings.Total; // SearchAsync already filters Active only

        var pending = await listingService.GetPendingAsync(cancellationToken);
        PendingListings = pending.Count;

        var users = await userService.GetAllAsync(cancellationToken);
        TotalUsers = users.Count;

        var cats = await categoryService.GetAllAsync(cancellationToken);
        TotalCategories = cats.Count;
    }
}
