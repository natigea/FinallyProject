using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class AddressService
    : GenericService<Address, AddressGetDto, AddressCreateDto, AddressUpdateDto>, IAddressService
{
    public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Addresses) { }

    public async Task<IReadOnlyList<AddressGetDto>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var addresses = await Repository.FindAsync(a => a.UserId == userId, cancellationToken);
        return Mapper.Map<IReadOnlyList<AddressGetDto>>(addresses);
    }

    public async Task<AddressGetDto> SetDefaultAsync(
        Guid addressId, Guid userId, CancellationToken cancellationToken = default)
    {
        var allAddresses = await Repository.FindAsync(a => a.UserId == userId, cancellationToken);

        foreach (var addr in allAddresses.Where(a => a.IsDefault))
        {
            addr.IsDefault = false;
            await Repository.UpdateAsync(addr, cancellationToken);
        }

        var target = allAddresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new NotFoundException(nameof(Address), addressId);

        target.IsDefault = true;
        await Repository.UpdateAsync(target, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<AddressGetDto>(target);
    }
}
