using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class CategoriesModel(ICategoryService categoryService) : PageModel
{
    public IReadOnlyList<CategoryGetDto> Categories { get; private set; } = [];

    [BindProperty] public string NewName { get; set; } = string.Empty;
    [BindProperty] public string NewDescription { get; set; } = string.Empty;
    [BindProperty] public string NewIcon { get; set; } = "bi-tag";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Categories = await categoryService.GetAllAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            TempData["ErrorMessage"] = "Название обязательно.";
            return RedirectToPage();
        }
        await categoryService.CreateAsync(new CategoryCreateDto(NewName, NewDescription, NewIcon), cancellationToken);
        TempData["SuccessMessage"] = $"Категория «{NewName}» создана.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Категория удалена.";
        return RedirectToPage();
    }
}
