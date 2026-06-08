using EcommersProject.BLL.DTOs;
using EcommersProject.BLL.Extensions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Context;
using EcommersProject.DAL.Extensions;
using EcommersProject.Hubs;
using EcommersProject.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using EcommersProject.Resources;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddScoped<FcmService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLocalization(o => o.ResourcesPath = "");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

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

    // Idempotent schema migrations (safe to run on existing DB)
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            -- Reviews table
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
            );

            -- VIP columns on Listings
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsVip' AND Object_ID = OBJECT_ID(N'Listings'))
                ALTER TABLE [Listings] ADD [IsVip] BIT NOT NULL DEFAULT 0;
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'VipExpiresAt' AND Object_ID = OBJECT_ID(N'Listings'))
                ALTER TABLE [Listings] ADD [VipExpiresAt] DATETIMEOFFSET NULL;

            -- Purchases table
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Purchases' AND xtype='U')
            CREATE TABLE [Purchases] (
                [Id]             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [OrderNumber]    NVARCHAR(50)     NOT NULL DEFAULT '',
                [Type]           NVARCHAR(20)     NOT NULL DEFAULT 'Delivery',
                [UserId]         UNIQUEIDENTIFIER NOT NULL,
                [ListingId]      UNIQUEIDENTIFIER NOT NULL,
                [ListingTitle]   NVARCHAR(500)    NOT NULL DEFAULT '',
                [ListingPrice]   DECIMAL(18,2)    NOT NULL DEFAULT 0,
                [DeliveryFee]    DECIMAL(18,2)    NOT NULL DEFAULT 0,
                [TotalAmount]    DECIMAL(18,2)    NOT NULL DEFAULT 0,
                [VipDays]        INT              NULL,
                [DeliveryAddress] NVARCHAR(500)   NULL,
                [DeliveryCity]   NVARCHAR(200)    NULL,
                [DeliveryPhone]  NVARCHAR(50)     NULL,
                [Status]         NVARCHAR(20)     NOT NULL DEFAULT 'Pending',
                [CardLast4]      NVARCHAR(4)      NULL,
                [CardHolder]     NVARCHAR(200)    NULL,
                [PaidAt]         DATETIMEOFFSET   NULL,
                [CreatedDate]    DATETIMEOFFSET   NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedDate]    DATETIMEOFFSET   NOT NULL DEFAULT GETUTCDATE(),
                [IsDeleted]      BIT              NOT NULL DEFAULT 0,
                CONSTRAINT [FK_Purchases_Users]    FOREIGN KEY ([UserId])    REFERENCES [Users]([Id]),
                CONSTRAINT [FK_Purchases_Listings] FOREIGN KEY ([ListingId]) REFERENCES [Listings]([Id])
            );

            -- Seller approval columns on Purchases
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'SellerId' AND Object_ID = OBJECT_ID(N'Purchases'))
                ALTER TABLE [Purchases] ADD [SellerId] UNIQUEIDENTIFIER NULL;
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'SellerApprovalStatus' AND Object_ID = OBJECT_ID(N'Purchases'))
                ALTER TABLE [Purchases] ADD [SellerApprovalStatus] NVARCHAR(20) NULL;

            -- Password reset columns on Users
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ResetToken' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [ResetToken] NVARCHAR(20) NULL;
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'ResetTokenExpiry' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [ResetTokenExpiry] DATETIMEOFFSET NULL;

            -- Photo column on Users
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'PhotoUrl' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [PhotoUrl] NVARCHAR(500) NULL;

            -- TitleKey column on Notifications (for i18n template keys)
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'TitleKey' AND Object_ID = OBJECT_ID(N'Notifications'))
                ALTER TABLE [Notifications] ADD [TitleKey] NVARCHAR(100) NULL;

            -- FCM token for push notifications
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'FcmToken' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [FcmToken] NVARCHAR(500) NULL;

            -- Condition column on Listings (New / Used)
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'Condition' AND Object_ID = OBJECT_ID(N'Listings'))
                ALTER TABLE [Listings] ADD [Condition] NVARCHAR(20) NULL;

            -- Email verification columns on Users
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsEmailVerified' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [IsEmailVerified] BIT NOT NULL DEFAULT 0;
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'EmailVerificationCode' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [EmailVerificationCode] NVARCHAR(10) NULL;
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'EmailCodeExpiry' AND Object_ID = OBJECT_ID(N'Users'))
                ALTER TABLE [Users] ADD [EmailCodeExpiry] DATETIMEOFFSET NULL;

            -- Notifications table
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notifications' AND xtype='U')
            CREATE TABLE [Notifications] (
                [Id]          UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
                [UserId]      UNIQUEIDENTIFIER NOT NULL,
                [Title]       NVARCHAR(500)    NOT NULL DEFAULT '',
                [Body]        NVARCHAR(MAX)    NOT NULL DEFAULT '',
                [Link]        NVARCHAR(500)    NULL,
                [IsRead]      BIT              NOT NULL DEFAULT 0,
                [PurchaseId]  UNIQUEIDENTIFIER NULL,
                [CreatedDate] DATETIMEOFFSET   NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedDate] DATETIMEOFFSET   NOT NULL DEFAULT GETUTCDATE(),
                [IsDeleted]   BIT              NOT NULL DEFAULT 0,
                CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
            );
        ");
    }
    catch { /* already applied or schema issue — ignore */ }

    // Create default admin
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var adminEmail = builder.Configuration["AdminDefaults:Email"]!;
    var adminPassword = builder.Configuration["AdminDefaults:Password"]!;
    var adminFirstName = builder.Configuration["AdminDefaults:FirstName"]!;
    var adminLastName = builder.Configuration["AdminDefaults:LastName"]!;
    await authService.CreateAdminAsync(adminEmail, adminPassword, adminFirstName, adminLastName);

    // Create hidden support listing for admin (used for support chat)
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT 1 FROM [Listings] WHERE [Title] = N'__SUPPORT__')
            BEGIN
                DECLARE @adminId UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [Users] WHERE [Role] = 2);
                DECLARE @catId   UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [Categories]);
                IF @adminId IS NOT NULL AND @catId IS NOT NULL
                    INSERT INTO [Listings] ([Id],[Title],[Description],[Price],[City],[ContactPhone],[Status],[CategoryId],[UserId],[IsVip],[DeliveryAvailable],[CreatedDate],[UpdatedDate],[IsDeleted])
                    VALUES (NEWID(), N'__SUPPORT__', N'Support', 0, N'', N'', 0, @catId, @adminId, 0, 0, GETUTCDATE(), GETUTCDATE(), 0);
            END
        ");
    }
    catch { }

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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.Run();
