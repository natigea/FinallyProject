namespace EcommersProject.DAL.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? PhotoUrl { get; set; }
    public string? ResetToken { get; set; }
    public DateTimeOffset? ResetTokenExpiry { get; set; }
    public string? FcmToken { get; set; }

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Conversation> BuyerConversations { get; set; } = new List<Conversation>();
    public ICollection<Conversation> SellerConversations { get; set; } = new List<Conversation>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
