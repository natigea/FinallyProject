namespace EcommersProject.BLL.DTOs;

public record PurchaseGetDto(
    Guid Id,
    string OrderNumber,
    string Type,
    string ListingTitle,
    Guid ListingId,
    decimal ListingPrice,
    decimal DeliveryFee,
    decimal TotalAmount,
    int? VipDays,
    string? DeliveryAddress,
    string? DeliveryCity,
    string? DeliveryPhone,
    string Status,
    string? SellerApprovalStatus,
    Guid? SellerId,
    string? SellerName,
    string? CardLast4,
    DateTimeOffset? PaidAt,
    DateTimeOffset CreatedDate,
    Guid UserId,
    string? BuyerName);

public record DeliveryCreateDto(
    Guid UserId,
    Guid ListingId,
    string ListingTitle,
    decimal ListingPrice,
    string DeliveryAddress,
    string DeliveryCity,
    string DeliveryPhone,
    string StripePaymentIntentId,
    string CardLast4);

public record VipCreateDto(
    Guid UserId,
    Guid ListingId,
    string ListingTitle,
    decimal ListingPrice,
    int VipDays,
    decimal VipPrice,
    string StripePaymentIntentId,
    string CardLast4);
