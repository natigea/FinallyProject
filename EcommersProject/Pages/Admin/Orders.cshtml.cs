using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OrdersModel(IListingService listingService) : PageModel
{
    public IReadOnlyList<ListingGetDto> Listings { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var (items, _) = await listingService.SearchAsync(
            new ListingSearchDto(Search, null, null, null, null, "newest", 1, 100),
            cancellationToken);
        Listings = items;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Объявление удалено.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCloseAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.CloseAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Объявление завершено.";
        return RedirectToPage();
    }
}
