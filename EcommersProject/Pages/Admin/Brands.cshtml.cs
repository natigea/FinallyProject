using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

// Repurposed: show all listings for admin moderation
[Authorize(Policy = "AdminOnly")]
public class BrandsModel(IListingService listingService) : PageModel
{
    public IReadOnlyList<ListingGetDto> Listings { get; private set; } = [];
    public int TotalCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var (items, total) = await listingService.SearchAsync(
            new ListingSearchDto(null, null, null, null, null, "newest", 1, 50),
            cancellationToken);
        Listings = items;
        TotalCount = total;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Объявление удалено.";
        return RedirectToPage();
    }
}
