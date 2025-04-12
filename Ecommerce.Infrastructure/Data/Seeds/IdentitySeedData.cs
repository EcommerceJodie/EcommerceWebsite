using Ecommerce.Core.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data.Seeds
{
    public static class IdentitySeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Tạo các role mặc định
            await CreateRolesAsync(roleManager);

            // Tạo tài khoản admin mặc định
            await CreateAdminUserAsync(userManager);
        }

        private static async Task CreateRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "Customer", "Manager" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName)
                    {
                        Description = $"Vai trò {roleName} của hệ thống",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private static async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "0987654321",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        public static async Task SeedCustomerAsync(UserManager<ApplicationUser> userManager, string email, string password, ApplicationDbContext context)
        {
            if (!userManager.Users.Any(u => u.Email == email))
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Customer",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer");

                    // Tạo Customer tương ứng
                    var customer = new Customer
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserId = user.Id,
                        IsActive = true
                    };

                    context.Customers.Add(customer);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
} 