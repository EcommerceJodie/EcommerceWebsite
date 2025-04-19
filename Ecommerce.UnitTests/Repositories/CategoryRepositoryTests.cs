using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.UnitTests.Repositories
{
    public class CategoryRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(_options);
            _repository = new CategoryRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategoryDescription = "Test Description",
                CategorySlug = "test-category",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();


            var result = await _repository.GetByIdAsync(categoryId);


            result.Should().NotBeNull();
            result.Id.Should().Be(categoryId);
            result.CategoryName.Should().Be("Test Category");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {

            var nonExistentId = Guid.NewGuid();


            var result = await _repository.GetByIdAsync(nonExistentId);


            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCategories()
        {

            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            var categories = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    CategoryName = "Category 1",
                    CategorySlug = "category-1",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    CategoryName = "Category 2",
                    CategorySlug = "category-2",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();


            var result = await _repository.GetAllAsync();


            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Select(c => c.CategoryName).Should().Contain(new[] { "Category 1", "Category 2" });
        }

        [Fact]
        public async Task Add_ShouldAddNewCategory()
        {

            var category = new Category
            {
                Id = Guid.NewGuid(),
                CategoryName = "New Category",
                CategorySlug = "new-category",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };


            _repository.Add(category);
            await _context.SaveChangesAsync();


            var savedCategory = await _context.Categories.FindAsync(category.Id);
            savedCategory.Should().NotBeNull();
            savedCategory.CategoryName.Should().Be("New Category");
        }

        [Fact]
        public async Task Update_ShouldUpdateExistingCategory()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Original Category",
                CategorySlug = "original-category",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();


            category.CategoryName = "Updated Category";
            category.CategorySlug = "updated-category";


            _repository.Update(category);
            await _context.SaveChangesAsync();


            var updatedCategory = await _context.Categories.FindAsync(categoryId);
            updatedCategory.Should().NotBeNull();
            updatedCategory.CategoryName.Should().Be("Updated Category");
            updatedCategory.CategorySlug.Should().Be("updated-category");
        }

        [Fact]
        public async Task Delete_ShouldRemoveCategoryFromDb()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Category to Delete",
                CategorySlug = "category-to-delete",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();


            _repository.Delete(category);
            await _context.SaveChangesAsync();


            var deletedCategory = await _context.Categories.FindAsync(categoryId);
            deletedCategory.Should().BeNull();
        }

        [Fact]
        public async Task CanQueryWithDbSet()
        {

            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            var categories = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    CategoryName = "Active Category",
                    CategorySlug = "active-category",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = Guid.NewGuid(),
                    CategoryName = "Inactive Category",
                    CategorySlug = "inactive-category",
                    IsActive = false,
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();


            var activeCategories = await _repository.Ts
                .Where(c => c.IsActive)
                .ToListAsync();


            activeCategories.Should().NotBeNull();
            activeCategories.Should().HaveCount(1);
            activeCategories[0].CategoryName.Should().Be("Active Category");
        }
    }
}
