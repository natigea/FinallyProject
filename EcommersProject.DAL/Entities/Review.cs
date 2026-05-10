namespace EcommersProject.DAL.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
