using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IProductImageService : IGenericService<ProductImageGetDto, ProductImageCreateDto, ProductImageUpdateDto>
{
    Task<IReadOnlyList<ProductImageGetDto>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default);
}
