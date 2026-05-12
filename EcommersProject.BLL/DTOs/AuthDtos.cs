namespace EcommersProject.BLL.DTOs;

public record LoginDto(string Email, string Password);

public record RegisterDto(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    bool IsSeller = false,
    string? ShopName = null,
    string? ShopDescription = null);

public record AuthResponseDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? ShopName);
