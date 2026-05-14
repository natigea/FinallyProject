using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly IUserService _users;
    private readonly IOrderService _orders;
    private readonly IAddressService _addresses;

    public AccountController(IAuthService auth, IUserService users,
        IOrderService orders, IAddressService addresses)
    {
        _auth = auth;
        _users = users;
        _orders = orders;
        _addresses = addresses;
    }

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
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _auth.LoginAsync(new LoginDto(vm.Email, vm.Password));
        if (result == null)
        {
            var existing = await _users.GetByEmailAsync(vm.Email.Trim().ToLowerInvariant());
            if (existing is { IsActive: false, Role: "Seller" })
                ModelState.AddModelError("", "Ваша заявка ожидает одобрения администратора.");
            else
                ModelState.AddModelError("", "Неверный email или пароль.");
            return View(vm);
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

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return result.Role switch
        {
            "Admin" => Redirect("/Admin/Dashboard"),
            "Seller" => Redirect("/Seller/Dashboard"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAfterLogin();
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            var result = await _auth.RegisterAsync(new RegisterDto(
                vm.Email, vm.FirstName, vm.LastName, vm.PhoneNumber, vm.Password,
                vm.IsSeller, vm.ShopName, vm.ShopDescription));

            if (result.Role == "Seller")
            {
                TempData["Info"] = "Ваша заявка отправлена на проверку администратору. Вы сможете войти после одобрения.";
                return RedirectToAction(nameof(Login));
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Id.ToString()),
                new(ClaimTypes.Email, result.Email),
                new(ClaimTypes.Name, $"{result.FirstName} {result.LastName}"),
                new(ClaimTypes.Role, result.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (success, error) = await _orders.CancelOrderAsync(id, userId);

        if (success)
            TempData["Success"] = "Заказ успешно отменён.";
        else
            TempData["Error"] = error;

        return RedirectToAction(nameof(OrderDetail), new { id });
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
        var user = await _users.GetByIdAsync(userId);
        var orders = await _orders.GetByUserAsync(userId);
        var addrs = await _addresses.GetByUserAsync(userId);

        return View(new ProfileViewModel { User = user, Orders = orders, Addresses = addrs });
    }

    public async Task<IActionResult> Orders()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", new { returnUrl = "/Account/Orders" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _orders.GetByUserAsync(userId);

        return View(orders);
    }

    public async Task<IActionResult> OrderDetail(Guid id)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", new { returnUrl = $"/Account/OrderDetail/{id}" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var order = await _orders.GetByIdAsync(id);
            if (order.UserId != userId) return Forbid();
            return View(order);
        }
        catch { return NotFound(); }
    }

    [HttpPost]
    public async Task<IActionResult> AddAddress(string line1, string line2, string city,
        string state, string postalCode, string country, bool isDefault)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _addresses.CreateAsync(new AddressCreateDto(userId, line1, line2 ?? "", city, state ?? "", postalCode ?? "", country, isDefault));
        TempData["Success"] = "Адрес добавлен.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        await _addresses.DeleteAsync(id);
        TempData["Success"] = "Адрес удалён.";
        return RedirectToAction(nameof(Profile));
    }

    private IActionResult RedirectAfterLogin()
    {
        if (User.IsInRole("Admin")) return Redirect("/Admin/Dashboard");
        if (User.IsInRole("Seller")) return Redirect("/Seller/Dashboard");
        return RedirectToAction("Index", "Home");
    }
}
