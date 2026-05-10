namespace EcommersProject.BLL.DTOs;

public record WishlistGetDto(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    string? ProductName,
    DateTimeOffset CreatedDate);

public record WishlistCreateDto(Guid UserId, Guid ProductId);

public record WishlistUpdateDto();
