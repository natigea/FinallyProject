using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Context;
using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommersProject.BLL.Services;

public class StatisticsService(AppDbContext context) : IStatisticsService
{
    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await context.Users.CountAsync(cancellationToken);
        var totalSellers = await context.Users.CountAsync(u => u.Role == UserRole.Seller, cancellationToken);
        var totalCustomers = await context.Users.CountAsync(u => u.Role == UserRole.Customer, cancellationToken);
        var totalProducts = await context.Products.CountAsync(cancellationToken);
        var totalOrders = await context.Orders.CountAsync(cancellationToken);
        var totalRevenue = totalOrders > 0
            ? await context.Orders.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;

        var pendingOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var processingOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Processing, cancellationToken);
        var shippedOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Shipped, cancellationToken);
        var deliveredOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
        var cancelledOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken);

        var recentOrders = await context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderDto(
                o.Id,
                o.OrderNumber,
                o.User != null ? o.User.FirstName + " " + o.User.LastName : "—",
                o.TotalAmount,
                o.Status,
                o.OrderDate))
            .ToListAsync(cancellationToken);

        var topProducts = await context.OrderItems
            .GroupBy(oi => new { oi.ProductId })
            .Select(g => new
            {
                g.Key.ProductId,
                TotalQty = g.Sum(oi => oi.Quantity),
                TotalRev = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.TotalRev)
            .Take(10)
            .Join(context.Products, t => t.ProductId, p => p.Id,
                (t, p) => new TopProductDto(p.Id, p.Name, t.TotalQty, t.TotalRev))
            .ToListAsync(cancellationToken);

        var cutoff = DateTimeOffset.UtcNow.AddMonths(-11);
        var rawMonthly = await context.Orders
            .Where(o => o.OrderDate >= cutoff)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(o => o.TotalAmount), Count = g.Count() })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync(cancellationToken);

        var monthlySales = rawMonthly.Select(m => new MonthlySalesDto(
            m.Year, m.Month,
            new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
            m.Revenue, m.Count))
            .ToList();

        return new AdminDashboardDto(
            totalUsers, totalSellers, totalCustomers, totalProducts,
            totalOrders, totalRevenue,
            pendingOrders, processingOrders, shippedOrders, deliveredOrders, cancelledOrders,
            recentOrders, topProducts, monthlySales);
    }

    public async Task<SellerDashboardDto> GetSellerDashboardAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var sellerProductIds = await context.Products
            .Where(p => p.SellerId == sellerId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var totalProducts = sellerProductIds.Count;

        if (totalProducts == 0)
            return EmptySeller(totalProducts);

        var totalRevenue = await context.OrderItems
            .Where(oi => sellerProductIds.Contains(oi.ProductId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var sellerOrderIds = await context.OrderItems
            .Where(oi => sellerProductIds.Contains(oi.ProductId))
            .Select(oi => oi.OrderId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalOrders = sellerOrderIds.Count;

        var statusCounts = await context.Orders
            .Where(o => sellerOrderIds.Contains(o.Id))
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int Get(string s) => statusCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

        var recentOrders = await context.Orders
            .Include(o => o.User)
            .Where(o => sellerOrderIds.Contains(o.Id))
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .Select(o => new RecentOrderDto(
                o.Id, o.OrderNumber,
                o.User != null ? o.User.FirstName + " " + o.User.LastName : "—",
                o.TotalAmount, o.Status, o.OrderDate))
            .ToListAsync(cancellationToken);

        var topProducts = await context.OrderItems
            .Where(oi => sellerProductIds.Contains(oi.ProductId))
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(oi => oi.Quantity), TotalRev = g.Sum(oi => oi.TotalPrice) })
            .OrderByDescending(p => p.TotalRev)
            .Take(10)
            .Join(context.Products, t => t.ProductId, p => p.Id,
                (t, p) => new TopProductDto(p.Id, p.Name, t.TotalQty, t.TotalRev))
            .ToListAsync(cancellationToken);

        var cutoff = DateTimeOffset.UtcNow.AddMonths(-11);
        var rawMonthly = await context.OrderItems
            .Where(oi => sellerProductIds.Contains(oi.ProductId))
            .Join(context.Orders.Where(o => o.OrderDate >= cutoff),
                oi => oi.OrderId, o => o.Id, (oi, o) => new { oi.TotalPrice, o.OrderDate, o.Id })
            .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.TotalPrice), Count = g.Select(x => x.Id).Distinct().Count() })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync(cancellationToken);

        var monthlySales = rawMonthly.Select(m => new MonthlySalesDto(
            m.Year, m.Month,
            new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy"),
            m.Revenue, m.Count))
            .ToList();

        return new SellerDashboardDto(
            totalProducts, totalOrders, totalRevenue,
            Get(OrderStatus.Pending), Get(OrderStatus.Processing),
            Get(OrderStatus.Shipped), Get(OrderStatus.Delivered), Get(OrderStatus.Cancelled),
            recentOrders, topProducts, monthlySales);
    }

    private static SellerDashboardDto EmptySeller(int totalProducts) =>
        new(totalProducts, 0, 0m, 0, 0, 0, 0, 0,
            new List<RecentOrderDto>(), new List<TopProductDto>(), new List<MonthlySalesDto>());
}
