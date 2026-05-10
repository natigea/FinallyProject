using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class ReviewService
    : GenericService<Review, ReviewGetDto, ReviewCreateDto, ReviewUpdateDto>, IReviewService
{
    public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Reviews) { }

    public async Task<IReadOnlyList<ReviewGetDto>> GetByProductAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        var reviews = await Repository.FindAsync(r => r.ProductId == productId, cancellationToken);
        return Mapper.Map<IReadOnlyList<ReviewGetDto>>(reviews);
    }

    public async Task<IReadOnlyList<ReviewGetDto>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var reviews = await Repository.FindAsync(r => r.UserId == userId, cancellationToken);
        return Mapper.Map<IReadOnlyList<ReviewGetDto>>(reviews);
    }
}
