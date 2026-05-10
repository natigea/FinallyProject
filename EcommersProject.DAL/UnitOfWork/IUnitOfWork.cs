using EcommersProject.DAL.Entities;
using EcommersProject.DAL.Interfaces;

namespace EcommersProject.DAL.UnitOfWork;

public interface IUnitOfWork
{
    IGenericRepository<Product> Products { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<Cart> Carts { get; }
    IGenericRepository<CartItem> CartItems { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<Wishlist> Wishlists { get; }
    IGenericRepository<Address> Addresses { get; }
    IGenericRepository<Payment> Payments { get; }
    IGenericRepository<Coupon> Coupons { get; }
    IGenericRepository<Brand> Brands { get; }
    IGenericRepository<ProductImage> ProductImages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
