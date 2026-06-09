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

// Render provides DATABASE_URL as postgres://user:pass@host/db
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var port = uri.Port > 0 ? uri.Port : 5432;
    var connStr = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;
}

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

    // Idempotent schema migrations (PostgreSQL — safe to run on existing DB)
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""Reviews"" (
                ""Id""          UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
                ""ListingId""   UUID NOT NULL,
                ""ReviewerId""  UUID NOT NULL,
                ""SellerId""    UUID NOT NULL,
                ""Rating""      INT NOT NULL DEFAULT 5,
                ""Comment""     TEXT NOT NULL DEFAULT '',
                ""CreatedDate"" TIMESTAMPTZ NOT NULL DEFAULT now(),
                ""UpdatedDate"" TIMESTAMPTZ NOT NULL DEFAULT now(),
                ""IsDeleted""   BOOLEAN NOT NULL DEFAULT FALSE,
                CONSTRAINT ""FK_Reviews_Listings"" FOREIGN KEY (""ListingId"") REFERENCES ""Listings""(""Id""),
                CONSTRAINT ""FK_Reviews_Reviewer"" FOREIGN KEY (""ReviewerId"") REFERENCES ""Users""(""Id""),
                CONSTRAINT ""FK_Reviews_Seller""   FOREIGN KEY (""SellerId"")   REFERENCES ""Users""(""Id"")
            );

            CREATE TABLE IF NOT EXISTS ""Purchases"" (
                ""Id""              UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
                ""OrderNumber""     VARCHAR(50)    NOT NULL DEFAULT '',
                ""Type""            VARCHAR(20)    NOT NULL DEFAULT 'Delivery',
                ""UserId""          UUID NOT NULL,
                ""ListingId""       UUID NOT NULL,
                ""ListingTitle""    VARCHAR(500)   NOT NULL DEFAULT '',
                ""ListingPrice""    NUMERIC(18,2)  NOT NULL DEFAULT 0,
                ""DeliveryFee""     NUMERIC(18,2)  NOT NULL DEFAULT 0,
                ""TotalAmount""     NUMERIC(18,2)  NOT NULL DEFAULT 0,
                ""VipDays""         INT            NULL,
                ""DeliveryAddress"" VARCHAR(500)   NULL,
                ""DeliveryCity""    VARCHAR(200)   NULL,
                ""DeliveryPhone""   VARCHAR(50)    NULL,
                ""Status""          VARCHAR(20)    NOT NULL DEFAULT 'Pending',
                ""CardLast4""       VARCHAR(4)     NULL,
                ""CardHolder""      VARCHAR(200)   NULL,
                ""PaidAt""          TIMESTAMPTZ    NULL,
                ""CreatedDate""     TIMESTAMPTZ    NOT NULL DEFAULT now(),
                ""UpdatedDate""     TIMESTAMPTZ    NOT NULL DEFAULT now(),
                ""IsDeleted""       BOOLEAN        NOT NULL DEFAULT FALSE,
                CONSTRAINT ""FK_Purchases_Users""    FOREIGN KEY (""UserId"")    REFERENCES ""Users""(""Id""),
                CONSTRAINT ""FK_Purchases_Listings"" FOREIGN KEY (""ListingId"") REFERENCES ""Listings""(""Id"")
            );

            CREATE TABLE IF NOT EXISTS ""Notifications"" (
                ""Id""          UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
                ""UserId""      UUID NOT NULL,
                ""Title""       VARCHAR(500) NOT NULL DEFAULT '',
                ""Body""        TEXT         NOT NULL DEFAULT '',
                ""Link""        VARCHAR(500) NULL,
                ""IsRead""      BOOLEAN      NOT NULL DEFAULT FALSE,
                ""PurchaseId""  UUID         NULL,
                ""CreatedDate"" TIMESTAMPTZ  NOT NULL DEFAULT now(),
                ""UpdatedDate"" TIMESTAMPTZ  NOT NULL DEFAULT now(),
                ""IsDeleted""   BOOLEAN      NOT NULL DEFAULT FALSE,
                CONSTRAINT ""FK_Notifications_Users"" FOREIGN KEY (""UserId"") REFERENCES ""Users""(""Id"")
            );

            ALTER TABLE ""Listings""      ADD COLUMN IF NOT EXISTS ""IsVip""        BOOLEAN NOT NULL DEFAULT FALSE;
            ALTER TABLE ""Listings""      ADD COLUMN IF NOT EXISTS ""VipExpiresAt"" TIMESTAMPTZ NULL;
            ALTER TABLE ""Listings""      ADD COLUMN IF NOT EXISTS ""Condition""    VARCHAR(20) NULL;
            ALTER TABLE ""Purchases""     ADD COLUMN IF NOT EXISTS ""SellerId""             UUID NULL;
            ALTER TABLE ""Purchases""     ADD COLUMN IF NOT EXISTS ""SellerApprovalStatus"" VARCHAR(20) NULL;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""ResetToken""              VARCHAR(20) NULL;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""ResetTokenExpiry""        TIMESTAMPTZ NULL;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""PhotoUrl""                VARCHAR(500) NULL;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""FcmToken""                VARCHAR(500) NULL;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""IsEmailVerified""         BOOLEAN NOT NULL DEFAULT FALSE;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""EmailVerificationCode""   VARCHAR(10) NULL;
            ALTER TABLE ""Users""         ADD COLUMN IF NOT EXISTS ""EmailCodeExpiry""         TIMESTAMPTZ NULL;
            ALTER TABLE ""Notifications"" ADD COLUMN IF NOT EXISTS ""TitleKey""                VARCHAR(100) NULL;
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
            INSERT INTO ""Listings"" (""Id"",""Title"",""Description"",""Price"",""City"",""ContactPhone"",""Status"",""CategoryId"",""UserId"",""IsVip"",""DeliveryAvailable"",""CreatedDate"",""UpdatedDate"",""IsDeleted"")
            SELECT gen_random_uuid(), '__SUPPORT__', 'Support', 0, '', '', 0,
                   (SELECT ""Id"" FROM ""Categories"" LIMIT 1),
                   (SELECT ""Id"" FROM ""Users"" WHERE ""Role"" = 2 LIMIT 1),
                   FALSE, FALSE, now(), now(), FALSE
            WHERE NOT EXISTS (SELECT 1 FROM ""Listings"" WHERE ""Title"" = '__SUPPORT__')
              AND EXISTS (SELECT 1 FROM ""Users"" WHERE ""Role"" = 2)
              AND EXISTS (SELECT 1 FROM ""Categories"");
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
