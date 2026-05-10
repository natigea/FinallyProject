using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class BrandConfiguration : BaseEntityConfiguration<Brand>
{
    public override void Configure(EntityTypeBuilder<Brand> builder)
    {
        base.Configure(builder);

        builder.Property(brand => brand.Name).HasMaxLength(200).IsRequired();
        builder.Property(brand => brand.Description).HasMaxLength(1000);

        builder.HasMany(brand => brand.Products)
            .WithOne(product => product.Brand)
            .HasForeignKey(product => product.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
