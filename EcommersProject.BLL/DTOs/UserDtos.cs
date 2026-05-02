namespace EcommersProject.BLL.DTOs;

public record UserGetDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    bool IsActive,
    string Role,
    DateTimeOffset CreatedDate);

public record UserCreateDto(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password);

public record UserUpdateDto(
    string FirstName,
    string LastName,
    string PhoneNumber);
