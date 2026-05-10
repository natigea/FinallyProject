using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class CouponConfiguration : BaseEntityConfiguration<Coupon>
{
    public override void Configure(EntityTypeBuilder<Coupon> builder)
    {
        base.Configure(builder);

        builder.Property(coupon => coupon.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(coupon => coupon.Code).IsUnique();
        builder.Property(coupon => coupon.DiscountAmount).HasPrecision(18, 2);
        builder.Property(coupon => coupon.DiscountPercent).HasPrecision(5, 2);
    }
}
