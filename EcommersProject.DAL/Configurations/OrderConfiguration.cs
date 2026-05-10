using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class OrderConfiguration : BaseEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        builder.Property(order => order.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.Property(order => order.Subtotal).HasPrecision(18, 2);
        builder.Property(order => order.DiscountAmount).HasPrecision(18, 2);
        builder.Property(order => order.ShippingAmount).HasPrecision(18, 2);
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.Status).HasMaxLength(50).IsRequired();

        builder.HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(order => order.User)
            .WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.ShippingAddress)
            .WithMany()
            .HasForeignKey(order => order.ShippingAddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(order => order.Coupon)
            .WithMany(coupon => coupon.Orders)
            .HasForeignKey(order => order.CouponId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
