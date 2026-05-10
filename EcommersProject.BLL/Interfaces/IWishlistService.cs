using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IWishlistService : IGenericService<WishlistGetDto, WishlistCreateDto, WishlistUpdateDto>
{
    Task<IReadOnlyList<WishlistGetDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WishlistGetDto?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    Task ToggleAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
