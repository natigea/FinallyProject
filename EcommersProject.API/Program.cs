using System.Text;
using EcommersProject.BLL.Extensions;
using EcommersProject.DAL.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ── OpenAPI (built-in .NET 10) ────────────────────────────────────────────────
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title       = "Alıs-Veris API";
        doc.Info.Version     = "v1";
        doc.Info.Description = "REST API для маркетплейса Alıs-Veris. " +
                               "Аутентификация: Bearer JWT — получите токен через POST /api/auth/login.";
        return Task.CompletedTask;
    });
});

builder.Services.AddDALServices(builder.Configuration);
builder.Services.AddBllServices();

var jwtKey      = builder.Configuration["Jwt:Key"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",     policy => policy.RequireRole("Admin"));
    options.AddPolicy("SellerOrAdmin", policy => policy.RequireRole("Admin", "Seller"));
});

var app = builder.Build();

// ── API docs — доступны в любом окружении включая Production ─────────────────
app.MapOpenApi(); // serves /openapi/v1.json

app.MapScalarApiReference(options =>
{
    options.Title             = "Alıs-Veris API";
    options.Theme             = ScalarTheme.Purple;
    options.DefaultHttpClient = new(ScalarTarget.JavaScript, ScalarClient.Fetch);
    options.Authentication    = new ScalarAuthenticationOptions
    {
        PreferredSecuritySchemes = ["Bearer"]
    };
});

// Root → Scalar UI
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
