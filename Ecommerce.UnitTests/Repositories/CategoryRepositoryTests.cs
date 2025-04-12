using System;
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
        private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllCategories()
        {
            // Arrange
            var options = GetDbContextOptions();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                context.Categories.AddRange(
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        CategoryName = "Category 1",
                        CategoryDescription = "Description 1",
                        CategorySlug = "category-1",
                        CategoryImageUrl = "image1.jpg"
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        CategoryName = "Category 2",
                        CategoryDescription = "Description 2",
                        CategorySlug = "category-2",
                        CategoryImageUrl = "image2.jpg"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Category>(context);
                var categories = repository.GetAll().ToList();

                // Assert
                categories.Should().NotBeNull();
                categories.Count.Should().Be(2);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                context.Categories.Add(
                    new Category
                    {
                        Id = categoryId,
                        CategoryName = "Test Category",
                        CategoryDescription = "Test Description",
                        CategorySlug = "test-category",
                        CategoryImageUrl = "test.jpg"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Category>(context);
                var category = await repository.GetByIdAsync(categoryId);

                // Assert
                category.Should().NotBeNull();
                category.CategoryName.Should().Be("Test Category");
                category.CategorySlug.Should().Be("test-category");
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var options = GetDbContextOptions();
            var nonExistentId = Guid.NewGuid();

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Category>(context);
                var category = await repository.GetByIdAsync(nonExistentId);

                // Assert
                category.Should().BeNull();
            }
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewCategory()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            
            var newCategory = new Category
            {
                Id = categoryId,
                CategoryName = "New Category",
                CategoryDescription = "New Description",
                CategorySlug = "new-category",
                CategoryImageUrl = "new.jpg"
            };

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Category>(context);
                await repository.AddAsync(newCategory);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var savedCategory = await context.Categories.FindAsync(categoryId);
                savedCategory.Should().NotBeNull();
                savedCategory.CategoryName.Should().Be("New Category");
                savedCategory.CategorySlug.Should().Be("new-category");
            }
        }

        [Fact]
        public async Task Update_ShouldUpdateCategory()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                context.Categories.Add(
                    new Category
                    {
                        Id = categoryId,
                        CategoryName = "Original Name",
                        CategoryDescription = "Original Description",
                        CategorySlug = "original-slug",
                        CategoryImageUrl = "original.jpg"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Category>(context);
                var category = await repository.GetByIdAsync(categoryId);
                category.CategoryName = "Updated Name";
                category.CategorySlug = "updated-slug";
                
                repository.Update(category);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var updatedCategory = await context.Categories.FindAsync(categoryId);
                updatedCategory.Should().NotBeNull();
                updatedCategory.CategoryName.Should().Be("Updated Name");
                updatedCategory.CategorySlug.Should().Be("updated-slug");
                updatedCategory.CategoryDescription.Should().Be("Original Description"); // Unchanged property
            }
        }

        [Fact]
        public async Task Delete_ShouldRemoveCategory()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                context.Categories.Add(
                    new Category
                    {
                        Id = categoryId,
                        CategoryName = "Category to Delete",
                        CategoryDescription = "Will be deleted",
                        CategorySlug = "category-to-delete",
                        CategoryImageUrl = "delete.jpg"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Category>(context);
                var category = await repository.GetByIdAsync(categoryId);
                
                repository.Delete(category);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var deletedCategory = await context.Categories.FindAsync(categoryId);
                deletedCategory.Should().BeNull();
            }
        }
    }
}