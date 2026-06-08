using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace EcommersProject.Pages.Seller;

[Authorize]
public class ProductsModel(IListingService listingService, ICategoryService categoryService, IStringLocalizer<SharedResource> L) : PageModel
{
    public IReadOnlyList<ListingGetDto> Listings { get; private set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; private set; } = [];
    public Guid UserId { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        UserId = GetUserId();
        Listings = await listingService.GetByUserAsync(UserId, cancellationToken);
        Categories = await categoryService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await listingService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Lst_Deleted"].Value;
        return RedirectToPage();
    }

    private Guid GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(str, out var id) ? id : Guid.Empty;
    }
}
