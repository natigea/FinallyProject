namespace EcommersProject.DAL.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = OrderStatus.Pending;

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid? ShippingAddressId { get; set; }
    public Address? ShippingAddress { get; set; }

    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public Payment? Payment { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
