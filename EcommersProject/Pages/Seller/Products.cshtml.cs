using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Seller;

[Authorize(Policy = "SellerOrAdmin")]
public class ProductsModel(
    IProductService productService,
    ICategoryService categoryService,
    IBrandService brandService) : PageModel
{
    public IReadOnlyList<ProductGetDto> Products { get; private set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; private set; } = [];
    public IReadOnlyList<BrandGetDto> Brands { get; private set; } = [];
    public Guid SellerId { get; private set; }

    [BindProperty] public string NewName { get; set; } = string.Empty;
    [BindProperty] public string NewDescription { get; set; } = string.Empty;
    [BindProperty] public string NewSku { get; set; } = string.Empty;
    [BindProperty] public decimal NewPrice { get; set; }
    [BindProperty] public int NewStockQuantity { get; set; }
    [BindProperty] public Guid NewCategoryId { get; set; }
    [BindProperty] public Guid NewBrandId { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        SellerId = GetSellerId();
        var all = await productService.GetAllAsync(cancellationToken);
        Products = all.Where(p => p.SellerId == SellerId).OrderByDescending(p => p.CreatedDate).ToList();
        Categories = await categoryService.GetAllAsync(cancellationToken);
        Brands = await brandService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        SellerId = GetSellerId();

        if (NewCategoryId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Выберите категорию товара.";
            return RedirectToPage();
        }

        if (NewBrandId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Выберите бренд товара.";
            return RedirectToPage();
        }

        await productService.CreateAsync(new ProductCreateDto(
            NewName, NewDescription, NewSku, NewPrice, NewStockQuantity,
            NewCategoryId, NewBrandId, SellerId, IsActive: false), cancellationToken);

        TempData["SuccessMessage"] = $"Товар «{NewName}» отправлен на проверку администратору.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await productService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Товар удалён.";
        return RedirectToPage();
    }

    private Guid GetSellerId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(str, out var id) ? id : Guid.Empty;
    }
}
