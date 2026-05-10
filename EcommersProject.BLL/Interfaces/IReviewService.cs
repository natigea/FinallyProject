using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IReviewService : IGenericService<ReviewGetDto, ReviewCreateDto, ReviewUpdateDto>
{
    Task<IReadOnlyList<ReviewGetDto>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewGetDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
