namespace EcommersProject.DAL.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? TitleKey { get; set; }
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public Guid? PurchaseId { get; set; }
}
