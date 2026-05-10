using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommersProject.DAL.Configurations;

public class WishlistConfiguration : BaseEntityConfiguration<Wishlist>
{
    public override void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        base.Configure(builder);

        builder.HasIndex(wishlist => new { wishlist.UserId, wishlist.ProductId }).IsUnique();

        builder.HasOne(wishlist => wishlist.User)
            .WithMany(user => user.Wishlists)
            .HasForeignKey(wishlist => wishlist.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wishlist => wishlist.Product)
            .WithMany(product => product.Wishlists)
            .HasForeignKey(wishlist => wishlist.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
