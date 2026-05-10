using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface ICouponService : IGenericService<CouponGetDto, CouponCreateDto, CouponUpdateDto>
{
    Task<CouponGetDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(string code, CancellationToken cancellationToken = default);
}
