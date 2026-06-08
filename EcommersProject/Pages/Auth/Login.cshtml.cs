using System.ComponentModel.DataAnnotations;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Resources;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace EcommersProject.Pages.Auth;

[AllowAnonymous]
public class LoginModel(
    IAuthService authService,
    EmailService emailService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<LoginModel> logger) : PageModel
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
            ErrorMessage = localizer["Auth_InvalidLogin"].Value;
            return Page();
        }

        if (result.Role == "Customer")
        {
            ErrorMessage = localizer["Auth_AccessDeniedPanel"].Value;
            return Page();
        }

        var code = await authService.GenerateTwoFactorCodeAsync(result.Id, cancellationToken);

        try
        {
            await emailService.SendVerificationCodeAsync(result.Email, code);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[2FA-Admin] Failed to send code to {Email}", result.Email);
            ErrorMessage = localizer["TwoFactor_SendError"].Value;
            return Page();
        }

        HttpContext.Session.SetString("2fa_userId", result.Id.ToString());
        HttpContext.Session.SetString("2fa_email", result.Email);
        HttpContext.Session.SetString("2fa_returnUrl", result.Role == "Admin" ? "/Admin/Dashboard" : "/Seller/Dashboard");

        return Redirect("/Account/TwoFactor");
    }
}
