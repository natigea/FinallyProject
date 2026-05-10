using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IAddressService : IGenericService<AddressGetDto, AddressCreateDto, AddressUpdateDto>
{
    Task<IReadOnlyList<AddressGetDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AddressGetDto> SetDefaultAsync(Guid addressId, Guid userId, CancellationToken cancellationToken = default);
}
