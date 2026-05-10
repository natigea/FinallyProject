using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IOrderService : IGenericService<OrderGetDto, OrderCreateDto, OrderUpdateDto>
{
    Task<IReadOnlyList<OrderGetDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<OrderGetDto> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
}
