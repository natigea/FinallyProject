using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController(IListingService listings, ICategoryService categories) : Controller
{
    public async Task<IActionResult> Index()
    {
        var (items, _) = await listings.SearchAsync(new ListingSearchDto(
            null, null, null, null, null, "newest", 1, 12));
        var cats = await categories.GetAllAsync();

        return View(new HomeViewModel
        {
            RecentListings = items,
            Categories = cats
        });
    }

    public IActionResult About() => View();

    [HttpGet]
    public IActionResult Contact() => View();

    [HttpPost]
    public IActionResult Contact(string name, string email, string subject, string message)
    {
        TempData["ContactSent"] = true;
        return RedirectToAction(nameof(Contact));
    }

    [Route("Error/{code?}")]
    public IActionResult Error(int? code)
    {
        ViewData["Code"] = code ?? 500;
        return View();
    }
}
