namespace EcommersProject.BLL.DTOs;

public record ProductGetDto(
    Guid Id,
    string Name,
    string Description,
    string Sku,
    decimal Price,
    decimal RatingAverage,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string? CategoryName,
    Guid BrandId,
    string? BrandName,
    IReadOnlyList<ProductImageGetDto> Images,
    DateTimeOffset CreatedDate);

public record ProductCreateDto(
    string Name,
    string Description,
    string Sku,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    Guid BrandId);

public record ProductUpdateDto(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    Guid BrandId);
