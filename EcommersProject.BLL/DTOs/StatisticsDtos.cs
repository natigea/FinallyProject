namespace EcommersProject.BLL.DTOs;

public record AdminDashboardDto(
    int TotalUsers,
    int TotalSellers,
    int TotalCustomers,
    int TotalProducts,
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CancelledOrders,
    IReadOnlyList<RecentOrderDto> RecentOrders,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<MonthlySalesDto> MonthlySales);

public record SellerDashboardDto(
    int TotalProducts,
    int TotalOrders,
    decimal TotalRevenue,
    int PendingOrders,
    int ProcessingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CancelledOrders,
    IReadOnlyList<RecentOrderDto> RecentOrders,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<MonthlySalesDto> MonthlySales);

public record RecentOrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTimeOffset OrderDate);

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    int TotalQuantitySold,
    decimal TotalRevenue);

public record MonthlySalesDto(
    int Year,
    int Month,
    string MonthName,
    decimal Revenue,
    int OrderCount);
