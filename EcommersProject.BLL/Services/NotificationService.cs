using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class NotificationService(IUnitOfWork uow) : INotificationService
{
    public async Task CreateAsync(Guid userId, string title, string body, string? link = null, Guid? purchaseId = null, string? titleKey = null)
    {
        var n = new Notification
        {
            UserId     = userId,
            Title      = title,
            Body       = body,
            Link       = link,
            PurchaseId = purchaseId,
            TitleKey   = titleKey
        };
        await uow.Notifications.AddAsync(n);
        await uow.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<NotificationGetDto>> GetByUserAsync(Guid userId)
    {
        var list = await uow.Notifications.FindAsync(n => n.UserId == userId);
        return list.OrderByDescending(n => n.CreatedDate)
                   .Select(ToDto)
                   .ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var list = await uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
        return list.Count;
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var unread = await uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
        foreach (var n in unread)
        {
            n.IsRead = true;
            await uow.Notifications.UpdateAsync(n);
        }
        if (unread.Any())
            await uow.SaveChangesAsync();
    }

    public async Task ClearPurchaseIdAsync(Guid purchaseId)
    {
        var list = await uow.Notifications.FindAsync(n => n.PurchaseId == purchaseId);
        foreach (var n in list)
        {
            n.PurchaseId = null;
            n.Link       = null;
            await uow.Notifications.UpdateAsync(n);
        }
        if (list.Any())
            await uow.SaveChangesAsync();
    }

    private static NotificationGetDto ToDto(Notification n) => new(
        n.Id, n.Title, n.Body, n.Link, n.IsRead, n.PurchaseId, n.CreatedDate, n.TitleKey);
}
