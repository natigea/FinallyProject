namespace EcommersProject.BLL.DTOs;

public record FavoriteGetDto(
    Guid Id,
    Guid UserId,
    Guid ListingId,
    string? ListingTitle,
    decimal? ListingPrice,
    string? ListingCity,
    string? ListingImage,
    DateTimeOffset CreatedDate);

public record FavoriteCreateDto(Guid UserId, Guid ListingId);
