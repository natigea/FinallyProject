using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class ProductImageService
    : GenericService<ProductImage, ProductImageGetDto, ProductImageCreateDto, ProductImageUpdateDto>, IProductImageService
{
    public ProductImageService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.ProductImages) { }

    public async Task<IReadOnlyList<ProductImageGetDto>> GetByProductAsync(
        Guid productId, CancellationToken cancellationToken = default)
    {
        var images = await Repository.FindAsync(i => i.ProductId == productId, cancellationToken);
        return Mapper.Map<IReadOnlyList<ProductImageGetDto>>(images);
    }
}
