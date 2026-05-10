namespace EcommersProject.DAL.Entities;

public class ProductImage : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
