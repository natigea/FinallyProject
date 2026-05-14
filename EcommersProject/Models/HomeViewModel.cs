using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class HomeViewModel
{
    public IReadOnlyList<ProductGetDto> FeaturedProducts { get; set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; set; } = [];
    public IReadOnlyList<ProductGetDto> NewArrivals { get; set; } = [];
}
