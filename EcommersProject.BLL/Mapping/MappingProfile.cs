using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.DAL.Entities;

namespace EcommersProject.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserGetDto>();
        CreateMap<UserCreateDto, User>()
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim().ToLowerInvariant()))
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Addresses, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.Carts, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());
        CreateMap<UserUpdateDto, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Addresses, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore())
            .ForMember(d => d.Carts, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());

        // Product
        CreateMap<Product, ProductGetDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images));
        CreateMap<ProductCreateDto, Product>()
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.RatingAverage, o => o.MapFrom(_ => 0m))
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Brand, o => o.Ignore())
            .ForMember(d => d.Images, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.CartItems, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());
        CreateMap<ProductUpdateDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Sku, o => o.Ignore())
            .ForMember(d => d.RatingAverage, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Brand, o => o.Ignore())
            .ForMember(d => d.Images, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.CartItems, o => o.Ignore())
            .ForMember(d => d.Wishlists, o => o.Ignore());

        // Category
        CreateMap<Category, CategoryGetDto>();
        CreateMap<CategoryCreateDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore());
        CreateMap<CategoryUpdateDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore());

        // Brand
        CreateMap<Brand, BrandGetDto>();
        CreateMap<BrandCreateDto, Brand>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore());
        CreateMap<BrandUpdateDto, Brand>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore());

        // Address
        CreateMap<Address, AddressGetDto>();
        CreateMap<AddressCreateDto, Address>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<AddressUpdateDto, Address>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        // Order
        CreateMap<Order, OrderGetDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
        CreateMap<OrderUpdateDto, Order>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderNumber, o => o.Ignore())
            .ForMember(d => d.OrderDate, o => o.Ignore())
            .ForMember(d => d.Subtotal, o => o.Ignore())
            .ForMember(d => d.DiscountAmount, o => o.Ignore())
            .ForMember(d => d.ShippingAmount, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CouponId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.ShippingAddress, o => o.Ignore())
            .ForMember(d => d.Coupon, o => o.Ignore())
            .ForMember(d => d.Payment, o => o.Ignore())
            .ForMember(d => d.Items, o => o.Ignore());

        // OrderItem
        CreateMap<OrderItem, OrderItemGetDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null));

        // Cart
        CreateMap<Cart, CartGetDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
        CreateMap<CartCreateDto, Cart>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.Items, o => o.Ignore());

        // CartItem
        CreateMap<CartItem, CartItemGetDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null));
        CreateMap<CartItemCreateDto, CartItem>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UnitPrice, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Cart, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());

        // Payment
        CreateMap<Payment, PaymentGetDto>();
        CreateMap<PaymentCreateDto, Payment>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => PaymentStatus.Pending))
            .ForMember(d => d.PaidAt, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Order, o => o.Ignore());
        CreateMap<PaymentUpdateDto, Payment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderId, o => o.Ignore())
            .ForMember(d => d.Amount, o => o.Ignore())
            .ForMember(d => d.Provider, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Order, o => o.Ignore());

        // ProductImage
        CreateMap<ProductImage, ProductImageGetDto>();
        CreateMap<ProductImageCreateDto, ProductImage>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());
        CreateMap<ProductImageUpdateDto, ProductImage>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProductId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());

        // Review
        CreateMap<Review, ReviewGetDto>();
        CreateMap<ReviewCreateDto, Review>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
        CreateMap<ReviewUpdateDto, Review>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProductId, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        // Wishlist
        CreateMap<Wishlist, WishlistGetDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : null));
        CreateMap<WishlistCreateDto, Wishlist>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());

        // Coupon
        CreateMap<Coupon, CouponGetDto>();
        CreateMap<CouponCreateDto, Coupon>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore());
        CreateMap<CouponUpdateDto, Coupon>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Code, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Orders, o => o.Ignore());
    }
}
