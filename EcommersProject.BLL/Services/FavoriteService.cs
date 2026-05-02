using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class FavoriteService(IUnitOfWork uow) : IFavoriteService
{
    public async Task<IReadOnlyList<FavoriteGetDto>> GetByUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        var favs = await uow.Favorites.FindAsync(
            f => f.UserId == userId, ct,
            f => f.Listing!, f => f.Listing!.Images);
        return favs.OrderByDescending(f => f.CreatedDate).Select(Map).ToList();
    }

    public async Task<bool> IsFavoriteAsync(
        Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var existing = await uow.Favorites.FindAsync(
            f => f.UserId == userId && f.ListingId == listingId, ct);
        return existing.Any();
    }

    public async Task<FavoriteGetDto> AddAsync(
        Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var existing = await uow.Favorites.FindAsync(
            f => f.UserId == userId && f.ListingId == listingId, ct);
        if (existing.Any())
            return Map(existing.First());

        var fav = new Favorite { UserId = userId, ListingId = listingId };
        await uow.Favorites.AddAsync(fav, ct);
        await uow.SaveChangesAsync(ct);

        var result = await uow.Favorites.FindAsync(
            f => f.Id == fav.Id, ct, f => f.Listing!, f => f.Listing!.Images);
        return Map(result.First());
    }

    public async Task RemoveAsync(
        Guid userId, Guid listingId, CancellationToken ct = default)
    {
        var existing = await uow.Favorites.FindAsync(
            f => f.UserId == userId && f.ListingId == listingId, ct);
        if (existing.FirstOrDefault() is { } fav)
        {
            await uow.Favorites.DeleteAsync(fav, ct);
            await uow.SaveChangesAsync(ct);
        }
    }

    private static FavoriteGetDto Map(Favorite f) => new(
        f.Id, f.UserId, f.ListingId,
        f.Listing?.Title,
        f.Listing?.Price,
        f.Listing?.City,
        f.Listing?.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
        f.CreatedDate);
}
