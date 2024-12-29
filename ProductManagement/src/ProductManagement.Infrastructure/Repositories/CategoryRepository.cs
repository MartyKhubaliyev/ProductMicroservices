using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Repositories;
using ProductManagement.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ProductManagement.Infrastructure.Repositories
{
    public class CategoryRepository : IRepository<Category>
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public CategoryRepository(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var cacheKey = $"Category_{id}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<Category>(cachedData);
            }

            // Fetch category from database
            var product = await _context.Categories.FindAsync(id);

            if (product != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(product), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }

            return product;
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            await _cache.SetStringAsync($"Category_{category.Id}", JsonSerializer.Serialize(category), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        public async Task UpdateAsync(Category category)
        {
            var cacheKey = $"Category_{category.Id}";
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(cacheKey);
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(category), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync($"Category_{id}");
            }
        }
    }
}
