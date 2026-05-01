namespace EcommersProject.DAL.Entities;

public class Review : BaseEntity
{
    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }

    public Guid ReviewerId { get; set; }
    public User? Reviewer { get; set; }

    public Guid SellerId { get; set; }
    public User? Seller { get; set; }

    public int Rating { get; set; }  // 1–5
    public string Comment { get; set; } = string.Empty;
}
