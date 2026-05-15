using AutoMapper;
using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Exceptions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Entities;
using EcommersProject.DAL.UnitOfWork;

namespace EcommersProject.BLL.Services;

public class OrderService
    : GenericService<Order, OrderGetDto, OrderCreateDto, OrderUpdateDto>, IOrderService
{
    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        : base(unitOfWork, mapper, unitOfWork.Orders) { }

    public override async Task<IReadOnlyList<OrderGetDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Repository.GetAllAsync(cancellationToken, o => o.Items, o => o.Payment!);
        return Mapper.Map<IReadOnlyList<OrderGetDto>>(entities);
    }

    public override async Task<OrderGetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var results = await Repository.FindAsync(
            o => o.Id == id, cancellationToken, o => o.Items, o => o.Payment!);
        var order = results.FirstOrDefault()
            ?? throw new NotFoundException(nameof(Order), id);
        return Mapper.Map<OrderGetDto>(order);
    }

    public override async Task<OrderGetDto> CreateAsync(
        OrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            OrderDate = DateTimeOffset.UtcNow,
            UserId = dto.UserId,
            ShippingAddressId = dto.ShippingAddressId,
            CouponId = dto.CouponId,
            Status = OrderStatus.Pending
        };

        decimal subtotal = 0;
        foreach (var itemDto in dto.Items)
        {
            var product = await UnitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), itemDto.ProductId);

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * itemDto.Quantity
            };
            order.Items.Add(orderItem);
            subtotal += orderItem.TotalPrice;
        }

        decimal discountAmount = 0;
        if (dto.CouponId.HasValue)
        {
            var coupon = await UnitOfWork.Coupons.GetByIdAsync(dto.CouponId.Value, cancellationToken);
            if (coupon != null
                && coupon.ValidFrom <= DateTimeOffset.UtcNow
                && coupon.ValidTo >= DateTimeOffset.UtcNow)
            {
                discountAmount = coupon.DiscountAmount > 0
                    ? coupon.DiscountAmount
                    : subtotal * coupon.DiscountPercent / 100m;
            }
        }

        order.Subtotal = subtotal;
        order.DiscountAmount = discountAmount;
        order.ShippingAmount = 0m;
        order.TotalAmount = subtotal - discountAmount + order.ShippingAmount;

        await UnitOfWork.Orders.AddAsync(order, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<OrderGetDto>(order);
    }

    public override async Task<OrderGetDto> UpdateAsync(
        Guid id, OrderUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var order = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
        Mapper.Map(dto, order);
        await Repository.UpdateAsync(order, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<OrderGetDto>(order);
    }

    public async Task<IReadOnlyList<OrderGetDto>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await Repository.FindAsync(
            o => o.UserId == userId, cancellationToken, o => o.Items, o => o.Payment!);
        return Mapper.Map<IReadOnlyList<OrderGetDto>>(orders);
    }

    public async Task<OrderGetDto> UpdateStatusAsync(
        Guid id, string status, CancellationToken cancellationToken = default)
    {
        var order = await Repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
        order.Status = status;
        await Repository.UpdateAsync(order, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return Mapper.Map<OrderGetDto>(order);
    }

    public async Task<(bool Success, string Error)> CancelOrderAsync(
        Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await Repository.GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
            return (false, "Заказ не найден.");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
            return (false, "Этот заказ нельзя отменить.");

        if (DateTimeOffset.UtcNow - order.OrderDate > TimeSpan.FromHours(1))
            return (false, "Время на отмену заказа истекло (1 час с момента оформления).");

        order.Status = OrderStatus.Cancelled;
        await Repository.UpdateAsync(order, cancellationToken);
        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return (true, string.Empty);
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}
