using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core.Interfaces;
using UrlShortener.Core.Services;
using UrlShortener.Infrastructure.Cache;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;
using System;

namespace UrlShortener.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database
            services.AddDbContext<UrlDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 15,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Add Redis cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "UrlShortener:";
            });

            // Register services
            services.AddScoped<IUrlRepository, UrlRepository>();
            services.AddScoped<IUrlCacheService, RedisCacheService>();
            services.AddScoped<ICodeGenerator, DefaultCodeGenerator>();

            return services;
        }
    }
}