using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EcommersProject.Pages.Auth;

[AllowAnonymous]
public class LoginModel(IAuthService authService) : PageModel
{
    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Required]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return User.IsInRole("Admin")
                ? RedirectToPage("/Admin/Dashboard")
                : RedirectToPage("/Seller/Dashboard");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await authService.LoginAsync(new LoginDto(Email, Password), cancellationToken);

        if (result is null)
        {
            ErrorMessage = "Неверный email или пароль.";
            return Page();
        }

        if (result.Role == "Customer")
        {
            ErrorMessage = "Доступ запрещён. Панель доступна только для администраторов и продавцов.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Id.ToString()),
            new(ClaimTypes.Email, result.Email),
            new(ClaimTypes.Name, $"{result.FirstName} {result.LastName}"),
            new(ClaimTypes.Role, result.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return result.Role == "Admin"
            ? RedirectToPage("/Admin/Dashboard")
            : RedirectToPage("/Seller/Dashboard");
    }
}
