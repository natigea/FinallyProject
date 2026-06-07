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

    [Required(ErrorMessage = "Введите заголовок")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "От 3 до 200 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "От 10 до 5000 символов")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите цену")]
    [Range(0, 10000000, ErrorMessage = "Цена должна быть неотрицательной")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Выберите город")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите контактный телефон")]
    [StringLength(30)]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите категорию")]
    public Guid CategoryId { get; set; }

    public IReadOnlyList<CategoryGetDto> Categories { get; set; } = [];
    public IReadOnlyList<ListingImageDto> ExistingImages { get; set; } = [];
    public List<IFormFile> NewImages { get; set; } = [];
    public List<Guid> DeleteImageIds { get; set; } = [];
}
