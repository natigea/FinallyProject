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
    IMessageService messages) : Controller
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

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Profile()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await users.GetByIdAsync(userId);
        var myListings = await listings.GetByUserAsync(userId);
        var favs = await favorites.GetByUserAsync(userId);
        var unread = await messages.GetUnreadCountAsync(userId);

        return View(new ProfileViewModel
        {
            User = user,
            MyListings = myListings,
            Favorites = favs,
            UnreadMessages = unread
        });
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
