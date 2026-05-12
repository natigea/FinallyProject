using EcommersProject.BLL.Interfaces;
using EcommersProject.BLL.Mapping;
using EcommersProject.BLL.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EcommersProject.BLL.Extensions;

public static class BllServiceRegistration
{
    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddValidatorsFromAssemblyContaining<MappingProfile>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        return services;
    }
}
