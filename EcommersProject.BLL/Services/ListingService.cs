using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace EcommersProject.BLL.Services;

public class ListingService(IUnitOfWork uow) : IListingService
{
    public async Task<ListingGetDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var results = await uow.Listings.FindAsync(
            l => l.Id == id, ct,
            l => l.Category!, l => l.User!, l => l.Images);
        var listing = results.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        return Map(listing);
    }

    public async Task<(IReadOnlyList<ListingGetDto> Items, int Total)> SearchAsync(
        ListingSearchDto dto, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var all = await uow.Listings.FindAsync(
            l => l.Status == ListingStatus.Active &&
                 (dto.CategoryId == null || l.CategoryId == dto.CategoryId) &&
                 (string.IsNullOrEmpty(dto.City) || l.City == dto.City) &&
                 (dto.PriceMin == null || l.Price >= dto.PriceMin) &&
                 (dto.PriceMax == null || l.Price <= dto.PriceMax) &&
                 (!dto.OnlyVip || (l.IsVip && l.VipExpiresAt.HasValue && l.VipExpiresAt.Value > now)) &&
                 (!dto.OnlyWithPhoto || l.Images.Any()) &&
                 (string.IsNullOrEmpty(dto.Query) ||
                  l.Title.ToLower().Contains(dto.Query.ToLower()) ||
                  l.Description.ToLower().Contains(dto.Query.ToLower())),
            ct,
            l => l.Category!, l => l.User!, l => l.Images);
        var sorted = dto.SortBy switch
        {
            "price_asc"  => all.OrderByDescending(l => l.IsVip && l.VipExpiresAt > now).ThenBy(l => l.Price),
            "price_desc" => all.OrderByDescending(l => l.IsVip && l.VipExpiresAt > now).ThenByDescending(l => l.Price),
            _            => all.OrderByDescending(l => l.IsVip && l.VipExpiresAt > now).ThenByDescending(l => l.CreatedDate)
        };

        var total = sorted.Count();
        var page  = dto.Page < 1 ? 1 : dto.Page;
        var size  = dto.PageSize < 1 ? 20 : dto.PageSize;
        var items = sorted.Skip((page - 1) * size).Take(size).ToList();

        return (items.Select(Map).ToList(), total);
    }

    // Admin version: searches across ALL statuses (Active + Pending + Closed)
    public async Task<(IReadOnlyList<ListingGetDto> Items, int Total)> AdminSearchAsync(
        ListingSearchDto dto, CancellationToken ct = default)
    {
        var all = await uow.Listings.FindAsync(
            l => (dto.CategoryId == null || l.CategoryId == dto.CategoryId) &&
                 (string.IsNullOrEmpty(dto.City) || l.City.ToLower().Contains(dto.City.ToLower())) &&
                 (dto.PriceMin == null || l.Price >= dto.PriceMin) &&
                 (dto.PriceMax == null || l.Price <= dto.PriceMax) &&
                 (string.IsNullOrEmpty(dto.Query) ||
                  l.Title.ToLower().Contains(dto.Query.ToLower()) ||
                  l.Description.ToLower().Contains(dto.Query.ToLower())),
            ct,
            l => l.Category!, l => l.User!, l => l.Images);

        var sorted = dto.SortBy switch
        {
            "price_asc"  => all.OrderBy(l => l.Price),
            "price_desc" => all.OrderByDescending(l => l.Price),
            _            => all.OrderByDescending(l => l.CreatedDate)
        };

        var total = sorted.Count();
        var page  = dto.Page < 1 ? 1 : dto.Page;
        var size  = dto.PageSize < 1 ? 20 : dto.PageSize;
        var items = sorted.Skip((page - 1) * size).Take(size).ToList();

        return (items.Select(Map).ToList(), total);
    }

    public async Task<IReadOnlyList<ListingGetDto>> GetByUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        var listings = await uow.Listings.FindAsync(
            l => l.UserId == userId, ct,
            l => l.Category!, l => l.User!, l => l.Images);
        return listings.OrderByDescending(l => l.CreatedDate).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ListingGetDto>> GetPendingAsync(CancellationToken ct = default)
    {
        var listings = await uow.Listings.FindAsync(
            l => l.Status == ListingStatus.Pending, ct,
            l => l.Category!, l => l.User!, l => l.Images);
        return listings.OrderBy(l => l.CreatedDate).Select(Map).ToList();
    }

    public async Task<ListingGetDto> CreateAsync(ListingCreateDto dto, CancellationToken ct = default)
    {
        var listing = new Listing
        {
            Title             = dto.Title,
            Description       = dto.Description,
            Price             = dto.Price,
            City              = dto.City,
            ContactPhone      = dto.ContactPhone,
            CategoryId        = dto.CategoryId,
            UserId            = dto.UserId,
            Status            = ListingStatus.Pending,
            DeliveryAvailable = dto.DeliveryAvailable
        };
        await uow.Listings.AddAsync(listing, ct);
        await uow.SaveChangesAsync(ct);

        var result = await uow.Listings.FindAsync(
            l => l.Id == listing.Id, ct,
            l => l.Category!, l => l.User!, l => l.Images);
        return Map(result.First());
    }

    public async Task<ListingGetDto> UpdateAsync(Guid id, ListingUpdateDto dto, bool isAdminEdit = false, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);

        listing.Title             = dto.Title;
        listing.Description       = dto.Description;
        listing.Price             = dto.Price;
        listing.City              = dto.City;
        listing.ContactPhone      = dto.ContactPhone;
        listing.CategoryId        = dto.CategoryId;
        listing.DeliveryAvailable = dto.DeliveryAvailable;

        if (!isAdminEdit)
            listing.Status = ListingStatus.Pending;

        await uow.Listings.UpdateAsync(listing, ct);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task SubmitForReviewAsync(Guid id, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        listing.Status = ListingStatus.Pending;
        await uow.Listings.UpdateAsync(listing, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task ApproveAsync(Guid id, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        listing.Status = ListingStatus.Active;
        await uow.Listings.UpdateAsync(listing, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task RejectAsync(Guid id, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        listing.Status = ListingStatus.Closed;
        await uow.Listings.UpdateAsync(listing, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task CloseAsync(Guid id, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        listing.Status = ListingStatus.Closed;
        await uow.Listings.UpdateAsync(listing, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task ReopenAsync(Guid id, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        listing.Status = ListingStatus.Active;
        await uow.Listings.UpdateAsync(listing, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entities = await uow.Listings.FindAsync(l => l.Id == id, ct);
        var listing = entities.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Listing), id);
        await uow.Listings.DeleteAsync(listing, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task AddImageAsync(Guid listingId, string imageUrl, CancellationToken ct = default)
    {
        var existing = await uow.ListingImages.FindAsync(i => i.ListingId == listingId, ct);
        var image = new ListingImage
        {
            ListingId = listingId,
            Url       = imageUrl,
            SortOrder = existing.Count
        };
        await uow.ListingImages.AddAsync(image, ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteImageAsync(Guid imageId, CancellationToken ct = default)
    {
        var image = await uow.ListingImages.GetByIdAsync(imageId, ct)
            ?? throw new NotFoundException(nameof(ListingImage), imageId);
        await uow.ListingImages.DeleteAsync(image, ct);
        await uow.SaveChangesAsync(ct);
    }

    private static ListingGetDto Map(Listing l)
    {
        var now = DateTimeOffset.UtcNow;
        var isVipActive = l.IsVip && l.VipExpiresAt.HasValue && l.VipExpiresAt.Value > now;
        return new(
            l.Id, l.Title, l.Description, l.Price, l.City, l.ContactPhone,
            l.Status.ToString(),
            l.CategoryId, l.Category?.Name, l.Category?.Icon, l.Category?.Slug,
            l.UserId, l.User != null ? l.User.FirstName + " " + l.User.LastName : null,
            l.User?.PhoneNumber,
            l.Images.OrderBy(i => i.SortOrder).Select(i => new ListingImageDto(i.Id, i.Url, i.SortOrder)).ToList(),
            l.CreatedDate,
            isVipActive,
            l.VipExpiresAt,
            l.DeliveryAvailable);
    }
}
