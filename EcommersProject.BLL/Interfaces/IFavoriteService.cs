using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IFavoriteService
{
    Task<IReadOnlyList<FavoriteGetDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsFavoriteAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task<FavoriteGetDto> AddAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task RemoveAsync(Guid userId, Guid listingId, CancellationToken ct = default);
}
