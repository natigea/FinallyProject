namespace EcommersProject.BLL.DTOs;

public record AddressGetDto(
    Guid Id,
    Guid UserId,
    string Line1,
    string Line2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault);

public record AddressCreateDto(
    Guid UserId,
    string Line1,
    string Line2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault);

public record AddressUpdateDto(
    string Line1,
    string Line2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault);
