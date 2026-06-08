using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class CouponsModel(ICategoryService categoryService, IStringLocalizer<SharedResource> L) : PageModel
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
            TempData["ErrorMessage"] = L["Admin_NameRequired"].Value;
            return RedirectToPage();
        }
        await categoryService.CreateAsync(new CategoryCreateDto(NewName, NewDescription, NewIcon), cancellationToken);
        TempData["SuccessMessage"] = L["Admin_CategoryCreated"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await categoryService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_CategoryDeleted"].Value;
        return RedirectToPage();
    }
}
