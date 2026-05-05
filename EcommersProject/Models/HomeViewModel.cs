using EcommersProject.BLL.DTOs;

namespace EcommersProject.Models;

public class HomeViewModel
{
    public IReadOnlyList<ListingGetDto> RecentListings { get; set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; set; } = [];
}
