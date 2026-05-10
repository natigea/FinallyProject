namespace EcommersProject.BLL.Interfaces;

public interface IGenericService<TGetDto, TCreateDto, TUpdateDto>
{
    Task<IReadOnlyList<TGetDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TGetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TGetDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default);
    Task<TGetDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
