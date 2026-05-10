namespace EcommersProject.BLL.DTOs;

public record CouponGetDto(
    Guid Id,
    string Code,
    decimal DiscountAmount,
    decimal DiscountPercent,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    int UsageLimit);

public record CouponCreateDto(
    string Code,
    decimal DiscountAmount,
    decimal DiscountPercent,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    int UsageLimit);

public record CouponUpdateDto(
    decimal DiscountAmount,
    decimal DiscountPercent,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    int UsageLimit);
