using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class ShopViewModel
{
    public IReadOnlyList<ProductGetDto> Products { get; set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; set; } = [];
    public IReadOnlyList<BrandGetDto> Brands { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string? SearchQuery { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public string? Sort { get; set; }
}

public class ProductDetailViewModel
{
    public ProductGetDto Product { get; set; } = null!;
    public IReadOnlyList<ReviewGetDto> Reviews { get; set; } = [];
    public IReadOnlyList<ProductGetDto> RelatedProducts { get; set; } = [];
    public bool IsInWishlist { get; set; }
    public int CartQuantity { get; set; }
}
