using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PhoneNumber).HasMaxLength(30);
        builder.Property(user => user.IsActive).HasDefaultValue(true);

        builder.HasMany(user => user.Addresses)
            .WithOne(address => address.User)
            .HasForeignKey(address => address.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Orders)
            .WithOne(order => order.User)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.Carts)
            .WithOne(cart => cart.User)
            .HasForeignKey(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Reviews)
            .WithOne(review => review.User)
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Wishlists)
            .WithOne(wishlist => wishlist.User)
            .HasForeignKey(wishlist => wishlist.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
