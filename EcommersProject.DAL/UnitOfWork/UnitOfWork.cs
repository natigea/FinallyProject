using EcommersProject.DAL.Context;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.Interfaces;
using EcommersProject.DAL.Repositories;

namespace EcommersProject.DAL.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    private IGenericRepository<Product>? _products;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<User>? _users;
    private IGenericRepository<Order>? _orders;
    private IGenericRepository<OrderItem>? _orderItems;
    private IGenericRepository<Cart>? _carts;
    private IGenericRepository<CartItem>? _cartItems;
    private IGenericRepository<Review>? _reviews;
    private IGenericRepository<Wishlist>? _wishlists;
    private IGenericRepository<Address>? _addresses;
    private IGenericRepository<Payment>? _payments;
    private IGenericRepository<Coupon>? _coupons;
    private IGenericRepository<Brand>? _brands;
    private IGenericRepository<ProductImage>? _productImages;

    public IGenericRepository<Product> Products => _products ??= new GenericRepository<Product>(_context);
    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);
    public IGenericRepository<OrderItem> OrderItems => _orderItems ??= new GenericRepository<OrderItem>(_context);
    public IGenericRepository<Cart> Carts => _carts ??= new GenericRepository<Cart>(_context);
    public IGenericRepository<CartItem> CartItems => _cartItems ??= new GenericRepository<CartItem>(_context);
    public IGenericRepository<Review> Reviews => _reviews ??= new GenericRepository<Review>(_context);
    public IGenericRepository<Wishlist> Wishlists => _wishlists ??= new GenericRepository<Wishlist>(_context);
    public IGenericRepository<Address> Addresses => _addresses ??= new GenericRepository<Address>(_context);
    public IGenericRepository<Payment> Payments => _payments ??= new GenericRepository<Payment>(_context);
    public IGenericRepository<Coupon> Coupons => _coupons ??= new GenericRepository<Coupon>(_context);
    public IGenericRepository<Brand> Brands => _brands ??= new GenericRepository<Brand>(_context);
    public IGenericRepository<ProductImage> ProductImages => _productImages ??= new GenericRepository<ProductImage>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
