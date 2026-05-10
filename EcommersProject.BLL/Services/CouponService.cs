using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class CouponService
    : GenericService<Coupon, CouponGetDto, CouponCreateDto, CouponUpdateDto>, ICouponService
{
    public CouponService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Coupons) { }

    public async Task<CouponGetDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var coupons = await Repository.FindAsync(
            c => c.Code == code.Trim().ToUpperInvariant(), cancellationToken);
        var coupon = coupons.FirstOrDefault();
        return coupon is null ? null : Mapper.Map<CouponGetDto>(coupon);
    }

    public async Task<bool> ValidateAsync(string code, CancellationToken cancellationToken = default)
    {
        var coupons = await Repository.FindAsync(
            c => c.Code == code.Trim().ToUpperInvariant(), cancellationToken);
        var coupon = coupons.FirstOrDefault();
        if (coupon is null)
            return false;

        var now = DateTimeOffset.UtcNow;
        return coupon.ValidFrom <= now && coupon.ValidTo >= now;
    }
}
