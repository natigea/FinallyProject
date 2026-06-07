using EcommersProject.DAL.Entities;
using EcommersProject.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommersProject.DAL.UnitOfWork;

public interface IUnitOfWork
{
    DbContext Context { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Listing> Listings { get; }
    IGenericRepository<ListingImage> ListingImages { get; }
    IGenericRepository<Favorite> Favorites { get; }
    IGenericRepository<Conversation> Conversations { get; }
    IGenericRepository<Message> Messages { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<Purchase> Purchases { get; }
    IGenericRepository<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
