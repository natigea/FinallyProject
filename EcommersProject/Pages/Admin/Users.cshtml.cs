using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class UsersModel(
    IUserService userService,
    IStringLocalizer<SharedResource> localizer,
    IConfiguration configuration) : PageModel
{
    private IStringLocalizer<SharedResource> L => localizer;
    public string SuperAdminEmail { get; } = configuration["AdminDefaults:Email"]!.Trim().ToLowerInvariant();

    public IReadOnlyList<UserGetDto> Users { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var all = await userService.GetAllAsync(cancellationToken);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Search))
            filtered = filtered.Where(u =>
                u.Email.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(Search, StringComparison.OrdinalIgnoreCase));

        Users = filtered.OrderByDescending(u => u.CreatedDate).ToList();
    }

    public async Task<IActionResult> OnPostSetRoleAsync(Guid id, string role, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();

        if (currentUserId == id.ToString() && role != "Admin")
        {
            TempData["ErrorMessage"] = L["Admin_CannotDemoteSelf"].Value;
            return RedirectToPage();
        }

        var target = await userService.GetByIdAsync(id, cancellationToken);

        // Protect super admin from role changes
        if (target.Email.Trim().ToLowerInvariant() == SuperAdminEmail)
        {
            TempData["ErrorMessage"] = L["Admin_CannotDemoteSuperAdmin"].Value;
            return RedirectToPage();
        }

        // Only super admin can demote other admins
        if (role != "Admin" && target.Role == "Admin" && currentUserEmail != SuperAdminEmail)
        {
            TempData["ErrorMessage"] = L["Admin_CannotDemoteAdmin"].Value;
            return RedirectToPage();
        }

        await userService.SetRoleAsync(id, role, cancellationToken);
        TempData["SuccessMessage"] = role == "Admin"
            ? L["Admin_MakeAdminMsg"].Value
            : L["Admin_RemoveAdminMsg"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await userService.ActivateAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_UserActivatedMsg"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeactivateAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_UserDeactivatedMsg"].Value;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == id.ToString())
        {
            TempData["ErrorMessage"] = L["Admin_CannotDeleteSelf"].Value;
            return RedirectToPage();
        }

        var target = await userService.GetByIdAsync(id, cancellationToken);
        if (target.Email.Trim().ToLowerInvariant() == SuperAdminEmail)
        {
            TempData["ErrorMessage"] = L["Admin_CannotDeleteSuperAdmin"].Value;
            return RedirectToPage();
        }

        await userService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = L["Admin_UserDeletedMsg"].Value;
        return RedirectToPage();
    }
}
