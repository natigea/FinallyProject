namespace EcommersProject.DAL.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    /// <summary>Language-agnostic key used for localisation (Cat_{Slug}).</summary>
    public string Slug { get; set; } = string.Empty;

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
