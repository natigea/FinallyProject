using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ProductsModel(
    IListingService listingService,
    ICategoryService categoryService,
    IStringLocalizer<SharedResource> localizer) : PageModel
{
    private IStringLocalizer<SharedResource> L => localizer;

    public IReadOnlyList<ListingGetDto> Listings        { get; private set; } = [];
    public IReadOnlyList<ListingGetDto> PendingListings { get; private set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories     { get; private set; } = [];

    [BindProperty(SupportsGet = true)] public string? Search         { get; set; }
    [BindProperty(SupportsGet = true)] public Guid?   CategoryFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string  StatusFilter   { get; set; } = "all";

    public int CountAll     { get; private set; }
    public int CountActive  { get; private set; }
    public int CountPending { get; private set; }
    public int CountClosed  { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var (all, _) = await listingService.AdminSearchAsync(
            new ListingSearchDto(Search, CategoryFilter, null, null, null, "newest", 1, 2000),
            cancellationToken);

        CountAll     = all.Count;
        CountActive  = all.Count(l => l.Status == "Active");
        CountPending = all.Count(l => l.Status == "Pending");
        CountClosed  = all.Count(l => l.Status == "Closed");

        Listings = StatusFilter switch
        {
            "active"  => all.Where(l => l.Status == "Active").ToList(),
            "pending" => all.Where(l => l.Status == "Pending").ToList(),
            "closed"  => all.Where(l => l.Status == "Closed").ToList(),
            _         => all
        };

        PendingListings = StatusFilter == "all"
            ? await listingService.GetPendingAsync(cancellationToken)
            : [];

        Categories = await categoryService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.ApproveAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_ApprovedMsg"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.RejectAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_RejectedMsg"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_DeletedListingMsg"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCloseAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.CloseAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_ClosedListingMsg"].Value;
        return RedirectToPage();
    }
}
