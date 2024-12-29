using ProductManagement.Infrastructure.DbContexts;
using ProductManagement.Infrastructure.Repositories;
using ProductManagement.Infrastructure.DependencyInjection;
using ProductManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using ProductService.API.Producer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<Product>), typeof(ProductRepository));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(provider => new KafkaProducer());

builder.Services.ImplementRateLimiting(builder);
builder.Services.ImplementCors();
builder.Services.AddRedis();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
