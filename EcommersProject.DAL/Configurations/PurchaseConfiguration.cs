using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class PurchaseConfiguration : BaseEntityConfiguration<Purchase>
{
    public override void Configure(EntityTypeBuilder<Purchase> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ListingTitle).HasMaxLength(200);
        builder.Property(p => p.ListingPrice).HasPrecision(18, 2);
        builder.Property(p => p.DeliveryFee).HasPrecision(18, 2);
        builder.Property(p => p.TotalAmount).HasPrecision(18, 2);

        // Listing → Purchases already cascades; prevent second cascade path User → Purchases
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Listing)
            .WithMany()
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Seller)
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
