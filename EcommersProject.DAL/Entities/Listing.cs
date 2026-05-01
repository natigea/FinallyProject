namespace EcommersProject.DAL.Entities;

public class Listing : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public ListingStatus Status { get; set; } = ListingStatus.Active;

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
