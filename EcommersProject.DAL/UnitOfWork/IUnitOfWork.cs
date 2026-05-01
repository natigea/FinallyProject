using EcommersProject.DAL.Entities;
using EcommersProject.DAL.Interfaces;

namespace EcommersProject.DAL.UnitOfWork;

public interface IUnitOfWork
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Listing> Listings { get; }
    IGenericRepository<ListingImage> ListingImages { get; }
    IGenericRepository<Favorite> Favorites { get; }
    IGenericRepository<Conversation> Conversations { get; }
    IGenericRepository<Message> Messages { get; }
    IGenericRepository<Review> Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
