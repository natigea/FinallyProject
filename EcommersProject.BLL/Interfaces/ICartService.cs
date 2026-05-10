using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface ICartService : IGenericService<CartGetDto, CartCreateDto, CartUpdateDto>
{
    Task<CartGetDto?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CartGetDto> AddItemAsync(Guid cartId, CartItemCreateDto dto, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid cartItemId, CancellationToken cancellationToken = default);
    Task<CartGetDto> UpdateItemQuantityAsync(Guid cartItemId, int quantity, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);
}
