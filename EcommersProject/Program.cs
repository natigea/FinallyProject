using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Extensions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Context;
using EcommersProject.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(o => o.ResourcesPath = "");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supported = ["ru", "az", "en"];
    options.SetDefaultCulture("ru")
           .AddSupportedCultures(supported)
           .AddSupportedUICultures(supported);
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

builder.Services.AddDALServices(builder.Configuration);
builder.Services.AddBllServices();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SellerOrAdmin", policy => policy.RequireRole("Admin", "Seller"));
});

builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
        options.Conventions.AuthorizeFolder("/Seller", "SellerOrAdmin");
        options.Conventions.AllowAnonymousToFolder("/Auth");
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await db.Database.EnsureCreatedAsync();
        _ = await db.Users.Select(u => new { u.PasswordHash, u.Role }).FirstOrDefaultAsync();
    }
    catch
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var adminEmail = builder.Configuration["AdminDefaults:Email"]!;
    var adminPassword = builder.Configuration["AdminDefaults:Password"]!;
    var adminFirstName = builder.Configuration["AdminDefaults:FirstName"]!;
    var adminLastName = builder.Configuration["AdminDefaults:LastName"]!;
    await authService.CreateAdminAsync(adminEmail, adminPassword, adminFirstName, adminLastName);

    var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
    var existingCategories = await categoryService.GetAllAsync();
    if (!existingCategories.Any())
    {
        (string Name, string Description)[] defaultCategories =
        [
            ("Техника", "Электроника, бытовая техника, гаджеты и устройства"),
            ("Одежда", "Мужская, женская и детская одежда"),
            ("Запчасти для автомобиля", "Запчасти, аксессуары и расходники для автомобилей"),
            ("Машинки для бритья/стрижки", "Электробритвы, машинки для стрижки волос и бороды"),
            ("Инструменты", "Ручные и электрические инструменты для дома и мастерской"),
        ];
        foreach (var (name, desc) in defaultCategories)
            await categoryService.CreateAsync(new CategoryCreateDto(name, desc));
    }

    var brandService = scope.ServiceProvider.GetRequiredService<IBrandService>();
    var existingBrands = await brandService.GetAllAsync();
    if (!existingBrands.Any())
    {
        (string Name, string Description)[] defaultBrands =
        [
            ("Samsung", "Южнокорейский производитель электроники"),
            ("Apple", "Американский производитель смартфонов и компьютеров"),
            ("Philips", "Нидерландский бренд бытовой техники и электроники"),
            ("Stanley", "Американский производитель инструментов"),
            ("Bosch", "Немецкий производитель инструментов и бытовой техники"),
            ("Adidas", "Немецкий бренд спортивной одежды"),
        ];
        foreach (var (name, desc) in defaultBrands)
            await brandService.CreateAsync(new BrandCreateDto(name, desc));
    }

    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
    if (!await db.Products.AnyAsync(p => !p.IsDeleted))
    {
        var categories = await categoryService.GetAllAsync();
        var brands = await brandService.GetAllAsync();

        Guid CatId(string name) => categories.First(c => c.Name == name).Id;
        Guid BrandId(string name) => brands.First(b => b.Name == name).Id;

        (string Name, string Desc, string Sku, decimal Price, int Stock, string Cat, string Brand)[] sampleProducts =
        [
            ("Samsung Galaxy A55", "6.6\" AMOLED, 8/256 ГБ, камера 50 МП", "SAM-A55-256", 899.99m, 20, "Техника", "Samsung"),
            ("Apple iPhone 15", "6.1\" Super Retina, 128 ГБ, чип A16 Bionic", "APL-IP15-128", 1399.99m, 10, "Техника", "Apple"),
            ("Philips Series 5000", "Бритва с триммером для бороды, мокрое/сухое бритьё", "PHL-S5000", 149.99m, 35, "Машинки для бритья/стрижки", "Philips"),
            ("Stanley FatMax Набор", "Набор из 210 инструментов в кейсе", "STN-FM-210", 349.99m, 15, "Инструменты", "Stanley"),
            ("Adidas Tiro 23", "Спортивные брюки унисекс, полиэстер", "ADI-TIRO23-M", 89.99m, 50, "Одежда", "Adidas"),
        ];

        foreach (var (name, desc, sku, price, stock, cat, brand) in sampleProducts)
        {
            await productService.CreateAsync(new ProductCreateDto(
                name, desc, sku, price, stock,
                CatId(cat), BrandId(brand),
                SellerId: null, IsActive: true));
        }
    }
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
