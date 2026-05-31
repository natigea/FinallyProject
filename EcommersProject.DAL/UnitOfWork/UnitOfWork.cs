using EcommersProject.DAL.Context;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.Interfaces;
using EcommersProject.DAL.Repositories;

namespace EcommersProject.DAL.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IGenericRepository<User>? _users;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Listing>? _listings;
    private IGenericRepository<ListingImage>? _listingImages;
    private IGenericRepository<Favorite>? _favorites;
    private IGenericRepository<Conversation>? _conversations;
    private IGenericRepository<Message>? _messages;
    private IGenericRepository<Review>? _reviews;
    private IGenericRepository<Purchase>? _purchases;
    private IGenericRepository<Notification>? _notifications;

    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(context);
    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(context);
    public IGenericRepository<Listing> Listings => _listings ??= new GenericRepository<Listing>(context);
    public IGenericRepository<ListingImage> ListingImages => _listingImages ??= new GenericRepository<ListingImage>(context);
    public IGenericRepository<Favorite> Favorites => _favorites ??= new GenericRepository<Favorite>(context);
    public IGenericRepository<Conversation> Conversations => _conversations ??= new GenericRepository<Conversation>(context);
    public IGenericRepository<Message> Messages => _messages ??= new GenericRepository<Message>(context);
    public IGenericRepository<Review> Reviews => _reviews ??= new GenericRepository<Review>(context);
    public IGenericRepository<Purchase> Purchases => _purchases ??= new GenericRepository<Purchase>(context);
    public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
