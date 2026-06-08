using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using EcommersProject.Resources;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController(
    IAuthService auth,
    IUserService users,
    IListingService listings,
    IPurchaseService purchases,
    IReviewService reviews,
    EmailService emailService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAfterLogin();
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await auth.LoginAsync(new LoginDto(vm.Email, vm.Password));
        if (result == null)
        {
            ModelState.AddModelError("", localizer["Acc_InvalidLogin"].Value);
            return View(vm);
        }

        var code = await auth.GenerateTwoFactorCodeAsync(result.Id);

        try
        {
            await emailService.SendVerificationCodeAsync(result.Email, code);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[2FA] Failed to send code to {Email}", result.Email);
            ModelState.AddModelError("", localizer["TwoFactor_SendError"].Value);
            return View(vm);
        }

        HttpContext.Session.SetString("2fa_userId", result.Id.ToString());
        HttpContext.Session.SetString("2fa_email", result.Email);
        HttpContext.Session.SetString("2fa_returnUrl", vm.ReturnUrl ?? "");

        TempData["Info"] = localizer["TwoFactor_CodeSent"].Value;
        return RedirectToAction(nameof(TwoFactor));
    }

    [HttpGet]
    public IActionResult TwoFactor()
    {
        var userId = HttpContext.Session.GetString("2fa_userId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction(nameof(Login));

        var email = HttpContext.Session.GetString("2fa_email") ?? "";
        return View(new TwoFactorViewModel { Email = MaskEmail(email) });
    }

    [HttpPost]
    public async Task<IActionResult> TwoFactor(TwoFactorViewModel vm)
    {
        var userIdStr = HttpContext.Session.GetString("2fa_userId");
        var email = HttpContext.Session.GetString("2fa_email") ?? "";
        var returnUrl = HttpContext.Session.GetString("2fa_returnUrl") ?? "";

        logger.LogInformation("[2FA] POST — userId from session: {UserId}", userIdStr ?? "NULL");
        logger.LogInformation("[2FA] POST — code from user (trimmed): '{Code}'", vm.Code?.Trim());

        if (string.IsNullOrEmpty(userIdStr))
        {
            logger.LogWarning("[2FA] No userId in session — redirecting to Login");
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            vm.Email = MaskEmail(email);
            return View(vm);
        }

        var userId = Guid.Parse(userIdStr);
        var result = await auth.VerifyTwoFactorCodeAsync(userId, vm.Code);

        if (result == null)
        {
            logger.LogWarning("[2FA] Code verification FAILED for userId={UserId}", userId);
            ModelState.AddModelError("", localizer["TwoFactor_InvalidCode"].Value);
            vm.Email = MaskEmail(email);
            return View(vm);
        }

        logger.LogInformation("[2FA] Code verified OK for userId={UserId}, signing in", userId);

        HttpContext.Session.Remove("2fa_userId");
        HttpContext.Session.Remove("2fa_email");
        HttpContext.Session.Remove("2fa_returnUrl");

        await SignInUser(result);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (result.Role == "Admin") return Redirect("/Admin/Dashboard");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> ResendTwoFactorCode()
    {
        var userIdStr = HttpContext.Session.GetString("2fa_userId");
        var email = HttpContext.Session.GetString("2fa_email") ?? "";

        if (string.IsNullOrEmpty(userIdStr))
            return RedirectToAction(nameof(Login));

        var userId = Guid.Parse(userIdStr);
        var code = await auth.GenerateTwoFactorCodeAsync(userId);

        try
        {
            await emailService.SendVerificationCodeAsync(email, code);
            TempData["Info"] = localizer["TwoFactor_CodeResent"].Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[2FA] Resend failed to {Email}", email);
            TempData["Error"] = localizer["TwoFactor_SendError"].Value;
        }

        return RedirectToAction(nameof(TwoFactor));
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectAfterLogin();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var result = await auth.RegisterAsync(new RegisterDto(
                vm.Email, vm.FirstName, vm.LastName, vm.PhoneNumber, vm.Password));

            var code = await auth.GenerateTwoFactorCodeAsync(result.Id);

            try
            {
                await emailService.SendVerificationCodeAsync(result.Email, code);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[2FA-Reg] Failed to send code to {Email}", result.Email);
                ModelState.AddModelError("", localizer["TwoFactor_SendError"].Value);
                return View(vm);
            }

            HttpContext.Session.SetString("2fa_userId", result.Id.ToString());
            HttpContext.Session.SetString("2fa_email", result.Email);
            HttpContext.Session.SetString("2fa_returnUrl", "");

            TempData["Info"] = localizer["TwoFactor_CodeSent"].Value;
            return RedirectToAction(nameof(TwoFactor));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var token = await auth.GeneratePasswordResetTokenAsync(vm.Email);
        TempData["ResetCode"] = token;
        TempData["ResetEmail"] = vm.Email;
        return RedirectToAction(nameof(ForgotPasswordSent));
    }

    [HttpGet]
    public IActionResult ForgotPasswordSent() => View();

    [HttpGet]
    public IActionResult ResetPassword(string? email = null)
        => View(new ResetPasswordViewModel { Email = email ?? TempData["ResetEmail"] as string ?? "" });

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var ok = await auth.ResetPasswordAsync(vm.Email, vm.Token, vm.NewPassword);
        if (!ok)
        {
            ModelState.AddModelError("", localizer["Acc_ResetCodeInvalid"].Value);
            return View(vm);
        }

        TempData["Info"] = localizer["Acc_PasswordChanged"].Value;
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string tab = "overview")
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });

        ViewData["HideNavSearch"] = true;
        ViewData["HideFooter"]    = true;

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user           = await users.GetByIdAsync(userId);
        var myListings     = await listings.GetByUserAsync(userId);
        var myPurchases    = await purchases.GetByUserAsync(userId);
        var incomingOrders = await purchases.GetBySellerAsync(userId);
        var myReviews      = await reviews.GetForSellerAsync(userId);
        var avgRating      = await reviews.GetAverageRatingAsync(userId);

        return View(new ProfilePageViewModel
        {
            User = user,
            EditForm = new ProfileEditViewModel
            {
                FirstName   = user.FirstName,
                LastName    = user.LastName,
                PhoneNumber = user.PhoneNumber,
                PhotoUrl    = user.PhotoUrl
            },
            MyListings      = myListings,
            Purchases       = myPurchases,
            IncomingOrders  = incomingOrders,
            ReviewsReceived = myReviews,
            AvgRating       = avgRating,
            ActiveTab       = tab
        });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileEditViewModel vm)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });

        if (!ModelState.IsValid)
        {
            TempData["PwdError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Profile), new { tab = "settings" });
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        string? photoUrl = vm.PhotoUrl;

        if (vm.Photo is { Length: > 0 })
        {
            var ext = Path.GetExtension(vm.Photo.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            {
                ModelState.AddModelError("Photo", localizer["Acc_PhotoFormatError"].Value);
                return View(vm);
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{userId}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await vm.Photo.CopyToAsync(stream);
            photoUrl = $"/uploads/avatars/{fileName}";
        }

        await users.UpdateAsync(userId, new UserUpdateDto(vm.FirstName, vm.LastName, vm.PhoneNumber, photoUrl));

        var updated = await users.GetByIdAsync(userId);
        var existingClaims = ((ClaimsIdentity)User.Identity!).Claims
            .Where(c => c.Type != ClaimTypes.Name)
            .Append(new Claim(ClaimTypes.Name, $"{updated.FirstName} {updated.LastName}"))
            .ToList();
        var identity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        TempData["Success"] = localizer["Acc_ProfileUpdated"].Value;
        return RedirectToAction(nameof(Profile), new { tab = "settings" });
    }

    [HttpGet]
    public async Task<IActionResult> RemovePhoto()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await users.GetByIdAsync(userId);
        await users.UpdateAsync(userId, new UserUpdateDto(user.FirstName, user.LastName, user.PhoneNumber, ""));
        return RedirectToAction(nameof(Profile), new { tab = "settings" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await users.DeleteAsync(userId);
        TempData["Info"] = localizer["Acc_AccountDeleted"].Value;
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        if (!ModelState.IsValid)
        {
            TempData["PwdError"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
            return RedirectToAction(nameof(Profile));
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ok = await auth.ChangePasswordAsync(userId, vm.CurrentPassword, vm.NewPassword);
        if (!ok)
        {
            TempData["PwdError"] = localizer["Acc_WrongPassword"].Value;
            return RedirectToAction(nameof(Profile));
        }

        TempData["PwdSuccess"] = localizer["Acc_PasswordChangedShort"].Value;
        return RedirectToAction(nameof(Profile), new { tab = "settings" });
    }

    private async Task SignInUser(AuthResponseDto result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Id.ToString()),
            new(ClaimTypes.Email, result.Email),
            new(ClaimTypes.Name, $"{result.FirstName} {result.LastName}"),
            new(ClaimTypes.Role, result.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
    }

    private IActionResult RedirectAfterLogin()
    {
        if (User.IsInRole("Admin")) return Redirect("/Admin/Dashboard");
        return RedirectToAction("Index", "Home");
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return "";
        var idx = email.IndexOf('@');
        if (idx <= 1) return email;
        return email[0] + new string('*', Math.Min(idx - 1, 5)) + email[idx..];
    }
}
