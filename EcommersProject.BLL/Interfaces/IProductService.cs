using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IProductService : IGenericService<ProductGetDto, ProductCreateDto, ProductUpdateDto>
{
    Task<IReadOnlyList<ProductGetDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductGetDto>> GetByBrandAsync(Guid brandId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ProductGetDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
