using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.DAL.Entities;

namespace EcommersProject.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserGetDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<UserCreateDto, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim().ToLowerInvariant()))
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Role, o => o.MapFrom(_ => UserRole.Customer))
            .ForMember(d => d.PhotoUrl, o => o.Ignore())
            .ForMember(d => d.ResetToken, o => o.Ignore())
            .ForMember(d => d.ResetTokenExpiry, o => o.Ignore())
            .ForMember(d => d.Listings, o => o.Ignore())
            .ForMember(d => d.Favorites, o => o.Ignore())
            .ForMember(d => d.BuyerConversations, o => o.Ignore())
            .ForMember(d => d.SellerConversations, o => o.Ignore())
            .ForMember(d => d.SentMessages, o => o.Ignore());

        CreateMap<UserUpdateDto, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Email, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.ResetToken, o => o.Ignore())
            .ForMember(d => d.ResetTokenExpiry, o => o.Ignore())
            .ForMember(d => d.Listings, o => o.Ignore())
            .ForMember(d => d.Favorites, o => o.Ignore())
            .ForMember(d => d.BuyerConversations, o => o.Ignore())
            .ForMember(d => d.SellerConversations, o => o.Ignore())
            .ForMember(d => d.SentMessages, o => o.Ignore())
            .ForMember(d => d.PhotoUrl, o => o.MapFrom((src, dest) =>
                src.PhotoUrl == null ? dest.PhotoUrl :
                src.PhotoUrl == "" ? null : src.PhotoUrl));

        // Category
        CreateMap<Category, CategoryGetDto>();
        CreateMap<CategoryCreateDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Listings, o => o.Ignore());
        CreateMap<CategoryUpdateDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.UpdatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.Listings, o => o.Ignore());
    }
}
