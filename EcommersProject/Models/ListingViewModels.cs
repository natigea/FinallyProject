using EcommersProject.BLL.DTOs;
using System.ComponentModel.DataAnnotations;

namespace EcommersProject.Models;

public class BrowseViewModel
{
    public IReadOnlyList<ListingGetDto> Listings { get; set; } = [];
    public IReadOnlyList<CategoryGetDto> Categories { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // filters
    public string? Query { get; set; }
    public Guid? CategoryId { get; set; }
    public string? City { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public string? SortBy { get; set; }
    public bool OnlyVip { get; set; }
    public bool OnlyWithPhoto { get; set; }

    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

public class ListingFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Val_TitleRequired")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Val_TitleLength")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_DescriptionRequired")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Val_DescriptionLength")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_PriceRequired")]
    [Range(0, 10000000, ErrorMessage = "Val_PricePositive")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Val_CityRequired")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_ContactPhoneRequired")]
    [StringLength(30)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Val_CategoryRequired")]
    public Guid CategoryId { get; set; }

    public string? Condition { get; set; }
    public bool DeliveryAvailable { get; set; } = true;

    public IReadOnlyList<CategoryGetDto> Categories { get; set; } = [];
    public IReadOnlyList<ListingImageDto> ExistingImages { get; set; } = [];
    public List<IFormFile> NewImages { get; set; } = [];
    public List<Guid> DeleteImageIds { get; set; } = [];
}
