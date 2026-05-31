namespace EcommersProject.DAL.Entities;

public class Purchase : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string Type { get; set; } = PurchaseType.Delivery;

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }

    public string ListingTitle { get; set; } = string.Empty;
    public decimal ListingPrice { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }

    public int? VipDays { get; set; }

    public string? DeliveryAddress { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryPhone { get; set; }

    public string Status { get; set; } = PurchaseStatus.Pending;
    public string? CardLast4 { get; set; }
    public string? CardHolder { get; set; }
    public DateTimeOffset? PaidAt { get; set; }

    public Guid? SellerId { get; set; }
    public User? Seller { get; set; }
    public string? SellerApprovalStatus { get; set; }
}
