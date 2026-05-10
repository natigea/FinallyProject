using EcommersProject.BLL.Extensions;
using EcommersProject.DAL.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDALServices(builder.Configuration);
builder.Services.AddBllServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
