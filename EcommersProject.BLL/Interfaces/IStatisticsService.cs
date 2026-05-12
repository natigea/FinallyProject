using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface IStatisticsService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<SellerDashboardDto> GetSellerDashboardAsync(Guid sellerId, CancellationToken cancellationToken = default);
}
