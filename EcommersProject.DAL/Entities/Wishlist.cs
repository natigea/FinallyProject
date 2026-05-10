namespace EcommersProject.DAL.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
