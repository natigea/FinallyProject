using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseGetDto> CreateDeliveryAsync(DeliveryCreateDto dto);
    Task<PurchaseGetDto> CreateVipAsync(VipCreateDto dto);
    Task<PurchaseGetDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PurchaseGetDto>> GetByUserAsync(Guid userId);
    Task<IReadOnlyList<PurchaseGetDto>> GetPendingApprovalBySellerAsync(Guid sellerId);
    Task<IReadOnlyList<PurchaseGetDto>> GetBySellerAsync(Guid sellerId);
    Task ApproveAsync(Guid purchaseId);
    Task RejectAsync(Guid purchaseId);
    Task<IReadOnlyList<PurchaseGetDto>> GetAllDeliveryAsync(CancellationToken cancellationToken = default);
}
