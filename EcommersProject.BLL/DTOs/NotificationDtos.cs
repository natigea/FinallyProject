namespace EcommersProject.BLL.DTOs;

public record NotificationGetDto(
    Guid Id,
    string Title,
    string Body,
    string? Link,
    bool IsRead,
    Guid? PurchaseId,
    DateTimeOffset CreatedDate,
    string? TitleKey = null);
