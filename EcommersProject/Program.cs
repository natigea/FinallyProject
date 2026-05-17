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
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
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
        options.Conventions.AllowAnonymousToFolder("/Auth");
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await db.Database.EnsureCreatedAsync();
        // Validate that key columns from the current schema exist
        _ = await db.Users.Select(u => new { u.PasswordHash, u.Role }).FirstOrDefaultAsync();
        _ = await db.Categories.Select(c => new { c.Icon, c.Slug }).FirstOrDefaultAsync();
        _ = await db.Listings.Select(l => new { l.Status, l.City }).FirstOrDefaultAsync();
    }
    catch
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    // Ensure Reviews table exists (added after initial schema)
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reviews' AND xtype='U')
            CREATE TABLE [Reviews] (
                [Id]          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [ListingId]   UNIQUEIDENTIFIER NOT NULL,
                [ReviewerId]  UNIQUEIDENTIFIER NOT NULL,
                [SellerId]    UNIQUEIDENTIFIER NOT NULL,
                [Rating]      INT NOT NULL DEFAULT 5,
                [Comment]     NVARCHAR(MAX) NOT NULL DEFAULT '',
                [CreatedDate] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedDate] DATETIMEOFFSET NOT NULL DEFAULT GETUTCDATE(),
                [IsDeleted]   BIT NOT NULL DEFAULT 0,
                CONSTRAINT [FK_Reviews_Listings] FOREIGN KEY ([ListingId]) REFERENCES [Listings]([Id]),
                CONSTRAINT [FK_Reviews_Reviewer] FOREIGN KEY ([ReviewerId]) REFERENCES [Users]([Id]),
                CONSTRAINT [FK_Reviews_Seller]   FOREIGN KEY ([SellerId])   REFERENCES [Users]([Id])
            )");
    }
    catch { /* table already exists or schema issue — ignore */ }

    // Create default admin
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var adminEmail = builder.Configuration["AdminDefaults:Email"]!;
    var adminPassword = builder.Configuration["AdminDefaults:Password"]!;
    var adminFirstName = builder.Configuration["AdminDefaults:FirstName"]!;
    var adminLastName = builder.Configuration["AdminDefaults:LastName"]!;
    await authService.CreateAdminAsync(adminEmail, adminPassword, adminFirstName, adminLastName);

    // Seed categories
    var categoryService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
    var existingCategories = await categoryService.GetAllAsync();
    if (!existingCategories.Any())
    {
        (string Name, string Description, string Icon, string Slug)[] defaultCategories =
        [
            ("Электроника",    "Телефоны, ноутбуки, планшеты и гаджеты", "bi-phone",    "electronics"),
            ("Одежда и обувь", "Мужская, женская и детская одежда",       "bi-bag",      "clothing"),
            ("Авто",           "Автомобили, мотоциклы, запчасти",         "bi-car-front","auto"),
            ("Недвижимость",   "Квартиры, дома, офисы, земля",            "bi-building", "realestate"),
            ("Работа",         "Вакансии и резюме",                       "bi-briefcase","jobs"),
            ("Услуги",         "Ремонт, строительство, репетиторство",    "bi-tools",    "services"),
            ("Для дома",       "Мебель, техника, декор",                  "bi-house",    "home"),
            ("Спорт и хобби",  "Спортивный инвентарь, музыка, книги",     "bi-bicycle",  "sports"),
            ("Животные",       "Домашние животные и товары для них",      "bi-heart",    "animals"),
            ("Бизнес",         "Оборудование, сырьё, партнёрство",        "bi-graph-up", "business"),
            ("Другое",         "Всё остальное",                           "bi-three-dots","other"),
        ];
        foreach (var (name, desc, icon, slug) in defaultCategories)
            await categoryService.CreateAsync(new CategoryCreateDto(name, desc, icon, slug));
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
