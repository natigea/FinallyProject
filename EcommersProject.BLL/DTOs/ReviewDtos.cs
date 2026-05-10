namespace EcommersProject.BLL.DTOs;

public record ReviewGetDto(
    Guid Id,
    Guid ProductId,
    Guid UserId,
    int Rating,
    string Comment,
    DateTimeOffset CreatedDate);

public record ReviewCreateDto(
    Guid ProductId,
    Guid UserId,
    int Rating,
    string Comment);

public record ReviewUpdateDto(
    int Rating,
    string Comment);
