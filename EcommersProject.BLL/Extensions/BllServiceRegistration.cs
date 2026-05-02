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

        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
