using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;

    public HomeController(IProductService products, ICategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<IActionResult> Index()
    {
        var all = await _products.GetAllAsync();
        var active = all.Where(p => p.IsActive).OrderByDescending(p => p.CreatedDate).ToList();
        var categories = await _categories.GetAllAsync();

        var vm = new HomeViewModel
        {
            FeaturedProducts = active.Take(4).ToList(),
            NewArrivals = active.Skip(4).Take(4).ToList(),
            Categories = categories
        };

        return View(vm);
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
