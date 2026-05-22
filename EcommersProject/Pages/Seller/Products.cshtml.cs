using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Seller;

[Authorize]
public class ProductsModel(IListingService listingService, ICategoryService categoryService) : PageModel
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
        TempData["SuccessMessage"] = "Объявление удалено.";
        return RedirectToPage();
    }

    private Guid GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(str, out var id) ? id : Guid.Empty;
    }
}
