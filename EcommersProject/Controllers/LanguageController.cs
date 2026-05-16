using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class LanguageController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string returnUrl = "/")
    {
        var supported = new[] { "ru", "az", "en" };
        if (!supported.Contains(culture))
            culture = "ru";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        return LocalRedirect(returnUrl);
    }
}
