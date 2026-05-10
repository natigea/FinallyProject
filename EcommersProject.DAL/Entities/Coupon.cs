namespace EcommersProject.DAL.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
    public int UsageLimit { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
