namespace EcommersProject.DAL.Entities;

public class Payment : BaseEntity
{
    public decimal Amount { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset PaidAt { get; set; }

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
}
