using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class PurchaseService(IUnitOfWork uow) : IPurchaseService
{
    public static decimal GetVipPrice(int days) => days switch
    {
        7  => 4.99m,
        14 => 7.99m,
        30 => 14.99m,
        _  => 4.99m
    };

    public async Task<PurchaseGetDto> CreateDeliveryAsync(DeliveryCreateDto dto)
    {
        var deliveryFee = Math.Max(Math.Round(dto.ListingPrice * 0.05m, 2), 1m);
        var total = dto.ListingPrice + deliveryFee;
        var orderNumber = GenerateOrderNumber();

        var listings = await uow.Listings.FindAsync(l => l.Id == dto.ListingId);
        var sellerId = listings.FirstOrDefault()?.UserId;

        var purchase = new Purchase
        {
            OrderNumber          = orderNumber,
            Type                 = PurchaseType.Delivery,
            UserId               = dto.UserId,
            ListingId            = dto.ListingId,
            ListingTitle         = dto.ListingTitle,
            ListingPrice         = dto.ListingPrice,
            DeliveryFee          = deliveryFee,
            TotalAmount          = total,
            DeliveryAddress      = dto.DeliveryAddress,
            DeliveryCity         = dto.DeliveryCity,
            DeliveryPhone        = dto.DeliveryPhone,
            Status               = PurchaseStatus.Completed,
            CardLast4            = dto.CardLast4,
            CardHolder           = dto.StripePaymentIntentId,
            PaidAt               = DateTimeOffset.UtcNow,
            SellerId             = sellerId,
            SellerApprovalStatus = "Pending"
        };

        await uow.Purchases.AddAsync(purchase);
        await uow.SaveChangesAsync();
        return ToDto(purchase);
    }

    public async Task<PurchaseGetDto> CreateVipAsync(VipCreateDto dto)
    {
        var orderNumber = GenerateOrderNumber();

        var purchase = new Purchase
        {
            OrderNumber  = orderNumber,
            Type         = PurchaseType.Vip,
            UserId       = dto.UserId,
            ListingId    = dto.ListingId,
            ListingTitle = dto.ListingTitle,
            ListingPrice = dto.ListingPrice,
            DeliveryFee  = 0m,
            TotalAmount  = dto.VipPrice,
            VipDays      = dto.VipDays,
            Status       = PurchaseStatus.Completed,
            CardLast4    = dto.CardLast4,
            CardHolder   = dto.StripePaymentIntentId,
            PaidAt       = DateTimeOffset.UtcNow
        };

        await uow.Purchases.AddAsync(purchase);

        var listings = await uow.Listings.FindAsync(l => l.Id == dto.ListingId);
        var listing = listings.FirstOrDefault();
        if (listing != null)
        {
            var now = DateTimeOffset.UtcNow;
            var currentExpiry = listing.IsVip && listing.VipExpiresAt.HasValue && listing.VipExpiresAt > now
                ? listing.VipExpiresAt.Value : now;
            listing.IsVip = true;
            listing.VipExpiresAt = currentExpiry.AddDays(dto.VipDays);
            await uow.Listings.UpdateAsync(listing);
        }

        await uow.SaveChangesAsync();
        return ToDto(purchase);
    }

    public async Task<PurchaseGetDto?> GetByIdAsync(Guid id)
    {
        var p = await uow.Purchases.GetByIdAsync(id);
        return p is null ? null : ToDto(p);
    }

    public async Task<IReadOnlyList<PurchaseGetDto>> GetByUserAsync(Guid userId)
    {
        var list = await uow.Purchases.FindAsync(p => p.UserId == userId);
        return list.Select(ToDto).OrderByDescending(p => p.CreatedDate).ToList();
    }

    public async Task<IReadOnlyList<PurchaseGetDto>> GetPendingApprovalBySellerAsync(Guid sellerId)
    {
        var list = await uow.Purchases.FindAsync(
            p => p.SellerId == sellerId && p.SellerApprovalStatus == "Pending");
        return list.Select(ToDto).OrderByDescending(p => p.CreatedDate).ToList();
    }

    public async Task ApproveAsync(Guid purchaseId)
    {
        var p = await uow.Purchases.GetByIdAsync(purchaseId);
        if (p == null) return;
        p.SellerApprovalStatus = "Approved";
        await uow.Purchases.UpdateAsync(p);
        await uow.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid purchaseId)
    {
        var p = await uow.Purchases.GetByIdAsync(purchaseId);
        if (p == null) return;
        p.SellerApprovalStatus = "Rejected";
        await uow.Purchases.UpdateAsync(p);
        await uow.SaveChangesAsync();
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    private static PurchaseGetDto ToDto(Purchase p) => new(
        p.Id, p.OrderNumber, p.Type,
        p.ListingTitle, p.ListingId,
        p.ListingPrice, p.DeliveryFee, p.TotalAmount,
        p.VipDays, p.DeliveryAddress, p.DeliveryCity,
        p.Status, p.SellerApprovalStatus, p.SellerId,
        p.CardLast4, p.PaidAt, p.CreatedDate, p.UserId);
}
