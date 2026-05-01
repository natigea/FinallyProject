using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class ListingImageConfiguration : BaseEntityConfiguration<ListingImage>
{
    public override void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        base.Configure(builder);

        builder.Property(i => i.Url).HasMaxLength(500).IsRequired();
        builder.Property(i => i.SortOrder);
    }
}
