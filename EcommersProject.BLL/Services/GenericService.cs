using AutoMapper;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.Interfaces;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public abstract class GenericService<TEntity, TGetDto, TCreateDto, TUpdateDto>
    : IGenericService<TGetDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IMapper Mapper;
    protected readonly IGenericRepository<TEntity> Repository;

    protected GenericService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IGenericRepository<TEntity> repository)
    {
        UnitOfWork = unitOfWork;
        Mapper = mapper;
        Repository = repository;
    }

    public virtual async Task<IReadOnlyList<TGetDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken);
        return Mapper.Map<IReadOnlyList<TGetDto>>(entities);
    }

    public virtual async Task<TGetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(typeof(TEntity).Name, id);
        return Mapper.Map<TGetDto>(entity);
    }

    public virtual async Task<TGetDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = Mapper.Map<TEntity>(dto);
        await Repository.AddAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<TGetDto>(entity);
    }

    public virtual async Task<TGetDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(typeof(TEntity).Name, id);
        Mapper.Map(dto, entity);
        await Repository.UpdateAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<TGetDto>(entity);
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(typeof(TEntity).Name, id);
        await Repository.DeleteAsync(entity, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
