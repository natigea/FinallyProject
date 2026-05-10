using EcommersProject.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommersProject.DAL.Seed;

public static class SeedData
{
    public static async Task SeedAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Set<Category>().AnyAsync(cancellationToken) || await context.Set<Brand>().AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Phones, gadgets, and accessories" },
            new() { Name = "Fashion", Description = "Apparel, shoes, and accessories" },
            new() { Name = "Home", Description = "Furniture, decor, and kitchen" }
        };

        var brands = new List<Brand>
        {
            new() { Name = "Skyline", Description = "Premium electronics" },
            new() { Name = "Northwind", Description = "Modern fashion" },
            new() { Name = "Contoso", Description = "Home essentials" }
        };

        await context.Set<Category>().AddRangeAsync(categories, cancellationToken);
        await context.Set<Brand>().AddRangeAsync(brands, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var products = new List<Product>
        {
            new()
            {
                Name = "Skyline X1 Smartphone",
                Description = "6.7-inch OLED, 5G, 256GB storage",
                Sku = "SKY-X1-256",
                Price = 799.99m,
                RatingAverage = 4.8m,
                StockQuantity = 120,
                IsActive = true,
                CategoryId = categories[0].Id,
                BrandId = brands[0].Id
            },
            new()
            {
                Name = "Northwind Streetwear Jacket",
                Description = "Water-resistant urban jacket",
                Sku = "NW-JKT-2024",
                Price = 149.50m,
                RatingAverage = 4.4m,
                StockQuantity = 80,
                IsActive = true,
                CategoryId = categories[1].Id,
                BrandId = brands[1].Id
            },
            new()
            {
                Name = "Contoso Smart Blender",
                Description = "Smart blender with presets and app control",
                Sku = "CT-BLND-900",
                Price = 129.99m,
                RatingAverage = 4.6m,
                StockQuantity = 60,
                IsActive = true,
                CategoryId = categories[2].Id,
                BrandId = brands[2].Id
            }
        };

        await context.Set<Product>().AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
