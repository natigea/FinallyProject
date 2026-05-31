using EcommersProject.BLL.DTOs;

namespace EcommersProject.BLL.Interfaces;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string title, string body, string? link = null, Guid? purchaseId = null);
    Task<IReadOnlyList<NotificationGetDto>> GetByUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAllReadAsync(Guid userId);
    Task ClearPurchaseIdAsync(Guid purchaseId);
}
