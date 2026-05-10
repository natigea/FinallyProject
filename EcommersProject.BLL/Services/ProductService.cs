using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class ProductService
    : GenericService<Product, ProductGetDto, ProductCreateDto, ProductUpdateDto>, IProductService
{
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Products) { }

    public override async Task<IReadOnlyList<ProductGetDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken,
            p => p.Category!, p => p.Brand!, p => p.Images);
        return Mapper.Map<IReadOnlyList<ProductGetDto>>(entities);
    }

    public override async Task<ProductGetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var results = await Repository.FindAsync(
            p => p.Id == id, cancellationToken,
            p => p.Category!, p => p.Brand!, p => p.Images);
        var entity = results.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Product), id);
        return Mapper.Map<ProductGetDto>(entity);
    }

    public async Task<IReadOnlyList<ProductGetDto>> GetByCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken = default)
    {
        var entities = await Repository.FindAsync(
            p => p.CategoryId == categoryId, cancellationToken,
            p => p.Category!, p => p.Brand!, p => p.Images);
        return Mapper.Map<IReadOnlyList<ProductGetDto>>(entities);
    }

    public async Task<IReadOnlyList<ProductGetDto>> GetByBrandAsync(
        Guid brandId, CancellationToken cancellationToken = default)
    {
        var entities = await Repository.FindAsync(
            p => p.BrandId == brandId, cancellationToken,
            p => p.Category!, p => p.Brand!, p => p.Images);
        return Mapper.Map<IReadOnlyList<ProductGetDto>>(entities);
    }

    public async Task<(IReadOnlyList<ProductGetDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await Repository.GetPagedAsync(
            pageNumber, pageSize, cancellationToken, null,
            p => p.Category!, p => p.Brand!, p => p.Images);
        return (Mapper.Map<IReadOnlyList<ProductGetDto>>(items), totalCount);
    }
}
