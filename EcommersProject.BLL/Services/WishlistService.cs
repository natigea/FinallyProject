using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class WishlistService
    : GenericService<Wishlist, WishlistGetDto, WishlistCreateDto, WishlistUpdateDto>, IWishlistService
{
    public WishlistService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Wishlists) { }

    public async Task<IReadOnlyList<WishlistGetDto>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await Repository.FindAsync(
            w => w.UserId == userId, cancellationToken, w => w.Product!);
        return Mapper.Map<IReadOnlyList<WishlistGetDto>>(items);
    }

    public async Task<WishlistGetDto?> GetByUserAndProductAsync(
        Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var items = await Repository.FindAsync(
            w => w.UserId == userId && w.ProductId == productId, cancellationToken, w => w.Product!);
        var item = items.FirstOrDefault();
        return item is null ? null : Mapper.Map<WishlistGetDto>(item);
    }

    public async Task ToggleAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var existing = await Repository.FindAsync(
            w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (existing.Count > 0)
        {
            await Repository.DeleteAsync(existing[0], cancellationToken);
        }
        else
        {
            var wishlist = new Wishlist { UserId = userId, ProductId = productId };
            await Repository.AddAsync(wishlist, cancellationToken);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
