using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewGetDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewGetDto>> GetForSellerAsync(Guid sellerId, CancellationToken ct = default);
    Task<bool> HasReviewedAsync(Guid reviewerId, Guid listingId, CancellationToken ct = default);
    Task<double?> GetAverageRatingAsync(Guid sellerId, CancellationToken ct = default);
    Task CreateAsync(ReviewCreateDto dto, CancellationToken ct = default);
}
