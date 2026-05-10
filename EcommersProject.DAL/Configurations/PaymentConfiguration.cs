using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class PaymentConfiguration : BaseEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.Property(payment => payment.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(payment => payment.Provider).HasMaxLength(100).IsRequired();
        builder.Property(payment => payment.Status).HasMaxLength(50).IsRequired();

        builder.HasOne(payment => payment.Order)
            .WithOne(order => order.Payment)
            .HasForeignKey<Payment>(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
