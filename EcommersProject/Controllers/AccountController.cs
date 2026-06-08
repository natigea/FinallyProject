using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController(
    IAuthService auth,
    IUserService users,
    IListingService listings,
    IFavoriteService favorites,
    IMessageService messages,
    IPurchaseService purchases,
    IReviewService reviews) : Controller
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
            ModelState.AddModelError("", "Неверный email или пароль.");
            return View(vm);
        }

        await SignInUser(result);

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        if (result.Role == "Admin") return Redirect("/Admin/Dashboard");
        return RedirectToAction("Index", "Home");
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

            await SignInUser(result);
            TempData["Success"] = "Добро пожаловать! Вы успешно зарегистрировались.";
            return RedirectToAction("Index", "Home");
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
            ModelState.AddModelError("", "Неверный или просроченный код. Запросите новый.");
            return View(vm);
        }

        TempData["Info"] = "Пароль успешно изменён. Войдите с новым паролем.";
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
                ModelState.AddModelError("Photo", "Допустимые форматы: JPG, PNG, WebP");
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

        // Refresh cookie with updated name
        var updated = await users.GetByIdAsync(userId);
        var existingClaims = ((System.Security.Claims.ClaimsIdentity)User.Identity!).Claims
            .Where(c => c.Type != ClaimTypes.Name)
            .Append(new Claim(ClaimTypes.Name, $"{updated.FirstName} {updated.LastName}"))
            .ToList();
        var identity = new System.Security.Claims.ClaimsIdentity(existingClaims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
            new System.Security.Claims.ClaimsPrincipal(identity),
            new Microsoft.AspNetCore.Authentication.AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        TempData["Success"] = "Профиль успешно обновлён.";
        return RedirectToAction(nameof(Profile), new { tab = "settings" });
    }

    [HttpGet]
    public async Task<IActionResult> RemovePhoto()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await users.GetByIdAsync(userId);
        // Pass "" (empty string) as PhotoUrl — mapping treats it as "clear to null"
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
        TempData["Info"] = "Ваш аккаунт был удалён.";
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
            TempData["PwdError"] = "Неверный текущий пароль.";
            return RedirectToAction(nameof(Profile));
        }

        TempData["PwdSuccess"] = "Пароль успешно изменён.";
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
}
