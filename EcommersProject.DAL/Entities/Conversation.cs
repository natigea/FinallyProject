namespace EcommersProject.DAL.Entities;

public class Conversation : BaseEntity
{
    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }

    public Guid BuyerId { get; set; }
    public User? Buyer { get; set; }

    public Guid SellerId { get; set; }
    public User? Seller { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
