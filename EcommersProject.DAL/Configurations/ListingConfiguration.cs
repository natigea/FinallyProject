using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class ListingConfiguration : BaseEntityConfiguration<Listing>
{
    public override void Configure(EntityTypeBuilder<Listing> builder)
    {
        base.Configure(builder);

        builder.Property(l => l.Title).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(5000);
        builder.Property(l => l.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(l => l.City).HasMaxLength(100).IsRequired();
        builder.Property(l => l.ContactPhone).HasMaxLength(30);
        builder.Property(l => l.Status).HasDefaultValue(ListingStatus.Active);

        builder.HasMany(l => l.Images)
            .WithOne(i => i.Listing)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict to avoid multiple cascade paths:
        // User → Listings (Cascade) + User → Favorites (Cascade) would create a cycle
        // if Listing → Favorites were also Cascade.
        builder.HasMany(l => l.Favorites)
            .WithOne(f => f.Listing)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Same reason: Listing → Conversations must be Restrict if User → Listings is Cascade
        // and there's any other cascade path to Conversations via User.
        builder.HasMany(l => l.Conversations)
            .WithOne(c => c.Listing)
            .HasForeignKey(c => c.ListingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
