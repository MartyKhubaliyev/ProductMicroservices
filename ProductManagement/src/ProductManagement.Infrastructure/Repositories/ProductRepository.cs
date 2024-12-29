using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Repositories;
using ProductManagement.Infrastructure.DbContexts;
using System.Text.Json;

namespace ProductManagement.Infrastructure.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public ProductRepository(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            var cacheKey = $"Product_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<Product>(cachedData);
            }

            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // Cache the product
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return product;
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            await _cache.SetStringAsync($"Product_{product.Id}", JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        public async Task UpdateAsync(Product product)
        {
            var cacheKey = $"Product_{product.Id}";

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync(cacheKey);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync($"Product_{id}");
            }
        }
    }
}