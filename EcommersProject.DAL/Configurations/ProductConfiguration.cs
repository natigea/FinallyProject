using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.Property(product => product.Name).HasMaxLength(250).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(4000);
        builder.Property(product => product.Sku).HasMaxLength(100).IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique();
        builder.Property(product => product.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(product => product.RatingAverage).HasPrecision(4, 2);
        builder.Property(product => product.IsActive).HasDefaultValue(true);

        builder.HasMany(product => product.Images)
            .WithOne(image => image.Product)
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(product => product.Reviews)
            .WithOne(review => review.Product)
            .HasForeignKey(review => review.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(product => product.OrderItems)
            .WithOne(orderItem => orderItem.Product)
            .HasForeignKey(orderItem => orderItem.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.CartItems)
            .WithOne(cartItem => cartItem.Product)
            .HasForeignKey(cartItem => cartItem.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.Wishlists)
            .WithOne(wishlist => wishlist.Product)
            .HasForeignKey(wishlist => wishlist.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
