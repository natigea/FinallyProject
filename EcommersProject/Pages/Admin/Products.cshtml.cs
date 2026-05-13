using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ProductsModel(
    IProductService productService,
    ICategoryService categoryService,
    IBrandService brandService,
    IUserService userService) : PageModel
{
    public IReadOnlyList<ProductGetDto> Products { get; private set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; private set; } = [];
    public IReadOnlyList<BrandGetDto> Brands { get; private set; } = [];
    public IReadOnlyList<UserGetDto> Sellers { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryFilter { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var allProducts = await productService.GetAllAsync(cancellationToken);
        var filtered = allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Search))
            filtered = filtered.Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase)
                                        || p.Sku.Contains(Search, StringComparison.OrdinalIgnoreCase));

        if (CategoryFilter.HasValue)
            filtered = filtered.Where(p => p.CategoryId == CategoryFilter.Value);

        Products = filtered.OrderByDescending(p => p.CreatedDate).ToList();

        Categories = await categoryService.GetAllAsync(cancellationToken);
        Brands = await brandService.GetAllAsync(cancellationToken);

        var allUsers = await userService.GetAllAsync(cancellationToken);
        Sellers = allUsers.Where(u => u.Role == "Seller").ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Товар удалён.";
        return RedirectToPage();
    }
}
