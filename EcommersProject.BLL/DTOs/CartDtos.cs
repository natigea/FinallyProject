namespace EcommersProject.BLL.DTOs;

public record CartGetDto(
    Guid Id,
    Guid UserId,
    IReadOnlyList<CartItemGetDto> Items);

public record CartCreateDto(Guid UserId);

public record CartUpdateDto();

public record CartItemGetDto(
    Guid Id,
    Guid CartId,
    Guid ProductId,
    string? ProductName,
    int Quantity,
    decimal UnitPrice);

public record CartItemCreateDto(
    Guid CartId,
    Guid ProductId,
    int Quantity);

public record CartItemUpdateDto(int Quantity);
