namespace EcommersProject.BLL.DTOs;

public record OrderGetDto(
    Guid Id,
    string OrderNumber,
    DateTimeOffset OrderDate,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TotalAmount,
    string Status,
    Guid UserId,
    Guid? ShippingAddressId,
    Guid? CouponId,
    IReadOnlyList<OrderItemGetDto> Items);

public record OrderCreateDto(
    Guid UserId,
    Guid? ShippingAddressId,
    Guid? CouponId,
    IReadOnlyList<OrderItemCreateDto> Items);

public record OrderUpdateDto(
    string Status,
    Guid? ShippingAddressId);

public record OrderItemGetDto(
    Guid Id,
    Guid OrderId,
    Guid ProductId,
    string? ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public record OrderItemCreateDto(
    Guid ProductId,
    int Quantity);
