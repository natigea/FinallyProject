namespace EcommersProject.DAL.Entities;

public class Favorite : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
