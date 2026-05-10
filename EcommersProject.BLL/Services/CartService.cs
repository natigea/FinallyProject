using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class CartService
    : GenericService<Cart, CartGetDto, CartCreateDto, CartUpdateDto>, ICartService
{
    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Carts) { }

    public override async Task<CartGetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var results = await Repository.FindAsync(c => c.Id == id, cancellationToken, c => c.Items);
        var cart = results.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Cart), id);
        return Mapper.Map<CartGetDto>(cart);
    }

    public async Task<CartGetDto?> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var carts = await Repository.FindAsync(c => c.UserId == userId, cancellationToken, c => c.Items);
        var cart = carts.FirstOrDefault();
        return cart is null ? null : Mapper.Map<CartGetDto>(cart);
    }

    public async Task<CartGetDto> AddItemAsync(
        Guid cartId, CartItemCreateDto dto, CancellationToken cancellationToken = default)
    {
        var carts = await Repository.FindAsync(c => c.Id == cartId, cancellationToken, c => c.Items);
        var cart = carts.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Cart), cartId);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity += dto.Quantity;
            await UnitOfWork.CartItems.UpdateAsync(existingItem, cancellationToken);
        }
        else
        {
            var product = await UnitOfWork.Products.GetByIdAsync(dto.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), dto.ProductId);

            var newItem = new CartItem
            {
                CartId = cartId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price
            };
            await UnitOfWork.CartItems.AddAsync(newItem, cancellationToken);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await Repository.FindAsync(c => c.Id == cartId, cancellationToken, c => c.Items);
        return Mapper.Map<CartGetDto>(updated.First());
    }

    public async Task RemoveItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var items = await UnitOfWork.CartItems.FindAsync(i => i.Id == cartItemId, cancellationToken);
        var item = items.FirstOrDefault()
            ?? throw new NotFoundException(nameof(CartItem), cartItemId);
        await UnitOfWork.CartItems.DeleteAsync(item, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CartGetDto> UpdateItemQuantityAsync(
        Guid cartItemId, int quantity, CancellationToken cancellationToken = default)
    {
        var items = await UnitOfWork.CartItems.FindAsync(i => i.Id == cartItemId, cancellationToken);
        var item = items.FirstOrDefault()
            ?? throw new NotFoundException(nameof(CartItem), cartItemId);

        if (quantity <= 0)
            await UnitOfWork.CartItems.DeleteAsync(item, cancellationToken);
        else
        {
            item.Quantity = quantity;
            await UnitOfWork.CartItems.UpdateAsync(item, cancellationToken);
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        var carts = await Repository.FindAsync(c => c.Id == item.CartId, cancellationToken, c => c.Items);
        return Mapper.Map<CartGetDto>(carts.First());
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var carts = await Repository.FindAsync(c => c.Id == cartId, cancellationToken, c => c.Items);
        var cart = carts.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Cart), cartId);

        foreach (var item in cart.Items.ToList())
            await UnitOfWork.CartItems.DeleteAsync(item, cancellationToken);

        await UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
