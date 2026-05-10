using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class AddressConfiguration : BaseEntityConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> builder)
    {
        base.Configure(builder);

        builder.Property(address => address.Line1).HasMaxLength(200).IsRequired();
        builder.Property(address => address.Line2).HasMaxLength(200);
        builder.Property(address => address.City).HasMaxLength(120).IsRequired();
        builder.Property(address => address.State).HasMaxLength(120);
        builder.Property(address => address.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(address => address.Country).HasMaxLength(120).IsRequired();

        builder.HasOne(address => address.User)
            .WithMany(user => user.Addresses)
            .HasForeignKey(address => address.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
