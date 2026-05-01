namespace EcommersProject.DAL.Entities;

public class ListingImage : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
