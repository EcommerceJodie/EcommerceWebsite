using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace Ecommerce.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            string projectDir = Directory.GetCurrentDirectory();
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            string[] configPaths = new[] 
            {
                Path.Combine(projectDir, "appsettings.json"),
                Path.Combine(projectDir, "..", "Ecommerce.API", "appsettings.json"),
                Path.Combine(projectDir, "..", "EcommerceWebsite", "appsettings.json")
            };
            
            string[] devConfigPaths = new[] 
            {
                Path.Combine(projectDir, $"appsettings.{environment}.json"),
                Path.Combine(projectDir, "..", "Ecommerce.API", $"appsettings.{environment}.json"),
                Path.Combine(projectDir, "..", "EcommerceWebsite", $"appsettings.{environment}.json")
            };
            
            string configPath = configPaths.FirstOrDefault(File.Exists) 
                ?? throw new FileNotFoundException("Could not find appsettings.json");
            
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configPath))
                .AddJsonFile(Path.GetFileName(configPath));
            
            string devConfigPath = devConfigPaths.FirstOrDefault(File.Exists);
            if (devConfigPath != null)
            {
                configBuilder.AddJsonFile(Path.GetFileName(devConfigPath));
            }
            
            var configuration = configBuilder.Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
} 