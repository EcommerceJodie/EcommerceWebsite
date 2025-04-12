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
    public class ProductRepositoryTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbContextOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllProducts()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                var category = new Category
                {
                    Id = categoryId,
                    CategoryName = "Test Category",
                    CategoryDescription = "Description",
                    CategorySlug = "test-category",
                    CategoryImageUrl = "test.jpg"
                };
                
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                
                context.Products.AddRange(
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "Product 1",
                        ProductDescription = "Description 1",
                        ProductSlug = "product-1",
                        ProductPrice = 9.99m,
                        ProductSku = "SKU1",
                        CategoryId = categoryId,
                        MetaTitle = "Meta 1",
                        MetaDescription = "Meta Desc 1"
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "Product 2",
                        ProductDescription = "Description 2",
                        ProductSlug = "product-2",
                        ProductPrice = 19.99m,
                        ProductSku = "SKU2",
                        CategoryId = categoryId,
                        MetaTitle = "Meta 2",
                        MetaDescription = "Meta Desc 2"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Product>(context);
                var products = repository.GetAll().ToList();

                // Assert
                products.Should().NotBeNull();
                products.Count.Should().Be(2);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                var category = new Category
                {
                    Id = categoryId,
                    CategoryName = "Test Category",
                    CategoryDescription = "Description",
                    CategorySlug = "test-category",
                    CategoryImageUrl = "test.jpg"
                };
                
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                
                context.Products.Add(
                    new Product
                    {
                        Id = productId,
                        ProductName = "Test Product",
                        ProductDescription = "Test Description",
                        ProductSlug = "test-product",
                        ProductPrice = 9.99m,
                        ProductSku = "TEST123",
                        CategoryId = categoryId,
                        MetaTitle = "Meta Title",
                        MetaDescription = "Meta Description"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Product>(context);
                var product = await repository.GetByIdAsync(productId);

                // Assert
                product.Should().NotBeNull();
                product.ProductName.Should().Be("Test Product");
                product.ProductSlug.Should().Be("test-product");
                product.ProductPrice.Should().Be(9.99m);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var options = GetDbContextOptions();
            var nonExistentId = Guid.NewGuid();

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Product>(context);
                var product = await repository.GetByIdAsync(nonExistentId);

                // Assert
                product.Should().BeNull();
            }
        }

        [Fact]
        public async Task AddAsync_ShouldAddNewProduct()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            // Setup category
            using (var context = new ApplicationDbContext(options))
            {
                var category = new Category
                {
                    Id = categoryId,
                    CategoryName = "Test Category",
                    CategoryDescription = "Description",
                    CategorySlug = "test-category",
                    CategoryImageUrl = "test.jpg"
                };
                
                context.Categories.Add(category);
                await context.SaveChangesAsync();
            }
            
            var newProduct = new Product
            {
                Id = productId,
                ProductName = "New Product",
                ProductDescription = "Description",
                ProductSlug = "new-product",
                ProductPrice = 29.99m,
                ProductSku = "NEW123",
                CategoryId = categoryId,
                MetaTitle = "New Meta",
                MetaDescription = "New Meta Description"
            };

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Product>(context);
                await repository.AddAsync(newProduct);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var savedProduct = await context.Products.FindAsync(productId);
                savedProduct.Should().NotBeNull();
                savedProduct.ProductName.Should().Be("New Product");
                savedProduct.ProductSlug.Should().Be("new-product");
                savedProduct.ProductPrice.Should().Be(29.99m);
            }
        }

        [Fact]
        public async Task Update_ShouldUpdateProduct()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                var category = new Category
                {
                    Id = categoryId,
                    CategoryName = "Test Category",
                    CategoryDescription = "Description",
                    CategorySlug = "test-category",
                    CategoryImageUrl = "test.jpg"
                };
                
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                
                context.Products.Add(
                    new Product
                    {
                        Id = productId,
                        ProductName = "Original Name",
                        ProductDescription = "Original Description",
                        ProductSlug = "original-slug",
                        ProductPrice = 9.99m,
                        ProductSku = "ORIG123",
                        CategoryId = categoryId,
                        MetaTitle = "Original Meta",
                        MetaDescription = "Original Meta Description"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Product>(context);
                var product = await repository.GetByIdAsync(productId);
                product.ProductName = "Updated Name";
                product.ProductSlug = "updated-slug";
                product.ProductPrice = 19.99m;
                
                repository.Update(product);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var updatedProduct = await context.Products.FindAsync(productId);
                updatedProduct.Should().NotBeNull();
                updatedProduct.ProductName.Should().Be("Updated Name");
                updatedProduct.ProductSlug.Should().Be("updated-slug");
                updatedProduct.ProductPrice.Should().Be(19.99m);
                updatedProduct.ProductDescription.Should().Be("Original Description"); // Unchanged property
            }
        }

        [Fact]
        public async Task Delete_ShouldRemoveProduct()
        {
            // Arrange
            var options = GetDbContextOptions();
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            // Seed the database
            using (var context = new ApplicationDbContext(options))
            {
                var category = new Category
                {
                    Id = categoryId,
                    CategoryName = "Test Category",
                    CategoryDescription = "Description",
                    CategorySlug = "test-category",
                    CategoryImageUrl = "test.jpg"
                };
                
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                
                context.Products.Add(
                    new Product
                    {
                        Id = productId,
                        ProductName = "Product to Delete",
                        ProductDescription = "Will be deleted",
                        ProductSlug = "product-to-delete",
                        ProductPrice = 9.99m,
                        ProductSku = "DELETE123",
                        CategoryId = categoryId,
                        MetaTitle = "Delete Meta",
                        MetaDescription = "Delete Meta Description"
                    }
                );
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new Repository<Product>(context);
                var product = await repository.GetByIdAsync(productId);
                
                repository.Delete(product);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new ApplicationDbContext(options))
            {
                var deletedProduct = await context.Products.FindAsync(productId);
                deletedProduct.Should().BeNull();
            }
        }
    }
} 