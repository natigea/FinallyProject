using System.Text;
using EcommersProject.BLL.Extensions;
using EcommersProject.BLL.Interfaces;
using EcommersProject.DAL.Context;
using EcommersProject.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ecommers API", Version = "v1" });
});

builder.Services.AddDALServices(builder.Configuration);
builder.Services.AddBllServices();

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
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
        // Проверяем совместимость схемы — если колонки новые отсутствуют, выбросит исключение
        _ = await db.Users.CountAsync();
    }
    catch
    {
        // Схема устарела — пересоздаём БД
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var adminEmail = builder.Configuration["AdminDefaults:Email"]!;
    var adminPassword = builder.Configuration["AdminDefaults:Password"]!;
    var adminFirstName = builder.Configuration["AdminDefaults:FirstName"]!;
    var adminLastName = builder.Configuration["AdminDefaults:LastName"]!;
    await authService.CreateAdminAsync(adminEmail, adminPassword, adminFirstName, adminLastName);
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommers API v1"));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
