namespace EcommersProject.BLL.DTOs;

public record ListingImageDto(Guid Id, string Url, int SortOrder);

public record ListingGetDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string City,
    string ContactPhone,
    string Status,
    Guid CategoryId,
    string? CategoryName,
    string? CategoryIcon,
    Guid UserId,
    string? UserName,
    string? UserPhone,
    List<ListingImageDto> Images,
    DateTimeOffset CreatedDate);

public record ListingCreateDto(
    string Title,
    string Description,
    decimal Price,
    string City,
    string ContactPhone,
    Guid CategoryId,
    Guid UserId);

public record ListingUpdateDto(
    string Title,
    string Description,
    decimal Price,
    string City,
    string ContactPhone,
    Guid CategoryId);

public record ListingSearchDto(
    string? Query,
    Guid? CategoryId,
    string? City,
    decimal? PriceMin,
    decimal? PriceMax,
    string? SortBy,
    int Page,
    int PageSize);
