using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IListingService
{
    Task<ListingGetDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ListingGetDto> Items, int Total)> SearchAsync(ListingSearchDto dto, CancellationToken ct = default);
    Task<(IReadOnlyList<ListingGetDto> Items, int Total)> AdminSearchAsync(ListingSearchDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ListingGetDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<ListingGetDto>> GetPendingAsync(CancellationToken ct = default);
    Task<ListingGetDto> CreateAsync(ListingCreateDto dto, CancellationToken ct = default);
    Task<ListingGetDto> UpdateAsync(Guid id, ListingUpdateDto dto, bool isAdminEdit = false, CancellationToken ct = default);
    Task SubmitForReviewAsync(Guid id, CancellationToken ct = default);
    Task ApproveAsync(Guid id, CancellationToken ct = default);
    Task RejectAsync(Guid id, CancellationToken ct = default);
    Task CloseAsync(Guid id, CancellationToken ct = default);
    Task ReopenAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task AddImageAsync(Guid listingId, string imageUrl, CancellationToken ct = default);
    Task DeleteImageAsync(Guid imageId, CancellationToken ct = default);
}
