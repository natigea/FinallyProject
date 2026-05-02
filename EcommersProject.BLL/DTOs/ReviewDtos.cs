namespace EcommersProject.BLL.DTOs;

public record ReviewGetDto(
    Guid Id,
    Guid ListingId,
    Guid ReviewerId,
    string ReviewerName,
    int Rating,
    string Comment,
    DateTimeOffset CreatedDate);

public record ReviewCreateDto(
    Guid ListingId,
    Guid ReviewerId,
    Guid SellerId,
    int Rating,
    string Comment);
