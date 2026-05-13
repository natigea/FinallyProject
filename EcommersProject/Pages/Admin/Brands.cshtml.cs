using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class BrandsModel(IBrandService brandService) : PageModel
{
    public IReadOnlyList<BrandGetDto> Brands { get; private set; } = [];

    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public string NewDescription { get; set; } = string.Empty;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Brands = await brandService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            TempData["ErrorMessage"] = "Название обязательно.";
            return RedirectToPage();
        }

        await brandService.CreateAsync(new BrandCreateDto(NewName, NewDescription), cancellationToken);
        TempData["SuccessMessage"] = $"Бренд «{NewName}» создан.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await brandService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Бренд удалён.";
        return RedirectToPage();
    }
}
