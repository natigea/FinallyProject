using EcommersProject.BLL.Extensions;
using EcommersProject.DAL.Context;
using EcommersProject.DAL.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDALServices(builder.Configuration);
builder.Services.AddBllServices();

var app = builder.Build();

// Create database schema on startup (dev convenience — replace with migrations when needed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
