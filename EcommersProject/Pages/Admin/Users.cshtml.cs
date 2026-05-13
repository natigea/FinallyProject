using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class UsersModel(IUserService userService, IAuthService authService) : PageModel
{
    public IReadOnlyList<UserGetDto> Users { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var all = await userService.GetAllAsync(cancellationToken);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(RoleFilter))
            filtered = filtered.Where(u => u.Role.Equals(RoleFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(Search))
            filtered = filtered.Where(u =>
                u.Email.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(Search, StringComparison.OrdinalIgnoreCase));

        Users = filtered.OrderByDescending(u => u.CreatedDate).ToList();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await userService.ActivateAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Пользователь активирован.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeactivateAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Пользователь деактивирован.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Пользователь удалён.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateSellerAsync(
        string email, string firstName, string lastName, string phone,
        string password, string shopName, string shopDescription,
        CancellationToken cancellationToken)
    {
        try
        {
            await authService.RegisterAsync(new RegisterDto(
                email, firstName, lastName, phone, password,
                IsSeller: true, ShopName: shopName, ShopDescription: shopDescription),
                cancellationToken);
            TempData["SuccessMessage"] = $"Продавец {email} создан.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}
