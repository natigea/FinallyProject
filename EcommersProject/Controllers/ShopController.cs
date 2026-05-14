using EcommersProject.BLL.Interfaces;
using EcommersProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommersProject.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ShopController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoryService _categories;
    private readonly IBrandService _brands;
    private readonly IReviewService _reviews;
    private readonly IWishlistService _wishlist;

    public ShopController(IProductService products, ICategoryService categories,
        IBrandService brands, IReviewService reviews, IWishlistService wishlist)
    {
        _products = products;
        _categories = categories;
        _brands = brands;
        _reviews = reviews;
        _wishlist = wishlist;
    }

    public async Task<IActionResult> Index(string? q, Guid? categoryId, Guid? brandId,
        string? sort, int page = 1)
    {
        var (items, total) = await _products.GetPagedAsync(page, 12);

        var filtered = items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(q))
            filtered = filtered.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                                        || p.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        if (categoryId.HasValue)
            filtered = filtered.Where(p => p.CategoryId == categoryId);
        if (brandId.HasValue)
            filtered = filtered.Where(p => p.BrandId == brandId);

        filtered = sort switch
        {
            "price_asc" => filtered.OrderBy(p => p.Price),
            "price_desc" => filtered.OrderByDescending(p => p.Price),
            "rating" => filtered.OrderByDescending(p => p.RatingAverage),
            _ => filtered
        };

        var vm = new ShopViewModel
        {
            Products = filtered.ToList(),
            TotalCount = total,
            PageNumber = page,
            Categories = await _categories.GetAllAsync(),
            Brands = await _brands.GetAllAsync(),
            SearchQuery = q,
            CategoryId = categoryId,
            BrandId = brandId,
            Sort = sort
        };

        return View(vm);
    }

    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var product = await _products.GetByIdAsync(id);
            var reviews = await _reviews.GetByProductAsync(id);
            var related = await _products.GetByCategoryAsync(product.CategoryId);

            bool inWishlist = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var entry = await _wishlist.GetByUserAndProductAsync(userId, id);
                inWishlist = entry != null;
            }

            var vm = new ProductDetailViewModel
            {
                Product = product,
                Reviews = reviews,
                RelatedProducts = related.Where(p => p.Id != id).Take(4).ToList(),
                IsInWishlist = inWishlist
            };

            return View(vm);
        }
        catch
        {
            return NotFound();
        }
    }
}
