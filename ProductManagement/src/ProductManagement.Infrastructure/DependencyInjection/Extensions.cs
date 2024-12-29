using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using AspNetCoreRateLimit;

namespace ProductManagement.Infrastructure.DependencyInjection
{
    public static class Extensions
    {
        public static void ImplementRateLimiting(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddInMemoryRateLimiting();
        }

        public static void ImplementCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                builder =>
                {
                    builder.WithOrigins("https://example.com")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            });
        }

        public static void AddRedis(this IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379"; // Replace with your Redis server address
                options.InstanceName = "ProductManagement_"; // Optional prefix for cache keys
            });
        }
    }
}
