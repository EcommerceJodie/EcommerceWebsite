using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Ecommerce.Infrastructure.Data
{
    public static class DatabaseServiceExtension
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfig = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"));
                redisConfig.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(redisConfig);
            });


            services.AddScoped<IRedisRepository, RedisRepository>();

            return services;
        }
    }
} 
