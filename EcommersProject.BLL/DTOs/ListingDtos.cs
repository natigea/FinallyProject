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
    string? CategorySlug,
    Guid UserId,
    string? UserName,
    string? UserPhone,
    List<ListingImageDto> Images,
    DateTimeOffset CreatedDate,
    bool IsVip,
    DateTimeOffset? VipExpiresAt);

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
    int PageSize,
    bool OnlyVip = false,
    bool OnlyWithPhoto = false);
