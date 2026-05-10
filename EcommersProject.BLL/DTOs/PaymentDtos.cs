namespace EcommersProject.BLL.DTOs;

public record PaymentGetDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Provider,
    string Status,
    DateTimeOffset PaidAt);

public record PaymentCreateDto(
    Guid OrderId,
    decimal Amount,
    string Provider);

public record PaymentUpdateDto(
    string Status,
    DateTimeOffset PaidAt);
