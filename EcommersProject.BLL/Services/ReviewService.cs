using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class ReviewService(IUnitOfWork uow) : IReviewService
{
    public async Task<IReadOnlyList<ReviewGetDto>> GetForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var reviews = await uow.Reviews.FindAsync(
            r => r.ListingId == listingId && !r.IsDeleted, ct,
            r => r.Reviewer!);
        return reviews.OrderByDescending(r => r.CreatedDate).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ReviewGetDto>> GetForSellerAsync(Guid sellerId, CancellationToken ct = default)
    {
        var reviews = await uow.Reviews.FindAsync(
            r => r.SellerId == sellerId && !r.IsDeleted, ct,
            r => r.Reviewer!);
        return reviews.OrderByDescending(r => r.CreatedDate).Select(Map).ToList();
    }

    public async Task<bool> HasReviewedAsync(Guid reviewerId, Guid listingId, CancellationToken ct = default)
    {
        var reviews = await uow.Reviews.FindAsync(
            r => r.ReviewerId == reviewerId && r.ListingId == listingId && !r.IsDeleted, ct);
        return reviews.Any();
    }

    public async Task<double?> GetAverageRatingAsync(Guid sellerId, CancellationToken ct = default)
    {
        var reviews = await uow.Reviews.FindAsync(
            r => r.SellerId == sellerId && !r.IsDeleted, ct);
        if (!reviews.Any()) return null;
        return reviews.Average(r => r.Rating);
    }

    public async Task CreateAsync(ReviewCreateDto dto, CancellationToken ct = default)
    {
        var review = new Review
        {
            ListingId  = dto.ListingId,
            ReviewerId = dto.ReviewerId,
            SellerId   = dto.SellerId,
            Rating     = Math.Clamp(dto.Rating, 1, 5),
            Comment    = dto.Comment
        };
        await uow.Reviews.AddAsync(review, ct);
        await uow.SaveChangesAsync(ct);
    }

    private static ReviewGetDto Map(Review r) => new(
        r.Id, r.ListingId, r.ReviewerId,
        r.Reviewer != null ? r.Reviewer.FirstName + " " + r.Reviewer.LastName : "—",
        r.Rating, r.Comment, r.CreatedDate);
}
