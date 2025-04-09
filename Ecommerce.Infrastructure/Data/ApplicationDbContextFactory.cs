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
            
            string[] configPaths = new[] 
            {
                Path.Combine(projectDir, "appsettings.json"),
                Path.Combine(projectDir, "..", "Ecommerce.API", "appsettings.json"),
                Path.Combine(projectDir, "..", "EcommerceWebsite", "appsettings.json")
            };
            
            string configPath = configPaths.FirstOrDefault(File.Exists) 
                ?? throw new FileNotFoundException("Could not find appsettings.json");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configPath))
                .AddJsonFile(Path.GetFileName(configPath))
                .Build();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
} 