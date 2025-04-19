using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.UnitTests.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(_options);
            _repository = new ProductRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Test Product",
                ProductDescription = "Test Description",
                ProductSlug = "test-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId,
                ProductStatus = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();


            var result = await _repository.GetByIdAsync(productId);


            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.ProductName.Should().Be("Test Product");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {

            var nonExistentId = Guid.NewGuid();


            var result = await _repository.GetByIdAsync(nonExistentId);


            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdWithIncludesAsync_ShouldIncludeCategory_WhenProductExists()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Test Product",
                ProductDescription = "Test Description",
                ProductSlug = "test-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId,
                ProductStatus = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();



            var result = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);


            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Category.Should().NotBeNull();
            result.Category.Id.Should().Be(categoryId);
            result.Category.CategoryName.Should().Be("Test Category");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllProducts()
        {

            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Product 1",
                    ProductSlug = "product-1",
                    ProductPrice = 99.99m,
                    ProductStock = 10,
                    ProductSku = "TST-001",
                    CategoryId = categoryId,
                    ProductStatus = ProductStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Product 2",
                    ProductSlug = "product-2",
                    ProductPrice = 199.99m,
                    ProductStock = 20,
                    ProductSku = "TST-002",
                    CategoryId = categoryId,
                    ProductStatus = ProductStatus.Active,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddAsync(category);
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();


            var result = await _repository.GetAllAsync();


            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Select(p => p.ProductName).Should().Contain(new[] { "Product 1", "Product 2" });
        }

        [Fact]
        public async Task Add_ShouldAddNewProduct()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "New Product",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId,
                ProductStatus = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();


            _repository.Add(product);
            await _context.SaveChangesAsync();


            var savedProduct = await _context.Products.FindAsync(product.Id);
            savedProduct.Should().NotBeNull();
            savedProduct.ProductName.Should().Be("New Product");
            savedProduct.CategoryId.Should().Be(categoryId);
        }

        [Fact]
        public async Task Update_ShouldUpdateExistingProduct()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Original Product",
                ProductSlug = "original-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId,
                ProductStatus = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();


            product.ProductName = "Updated Product";
            product.ProductSlug = "updated-product";
            product.ProductPrice = 199.99m;


            _repository.Update(product);
            await _context.SaveChangesAsync();


            var updatedProduct = await _context.Products.FindAsync(productId);
            updatedProduct.Should().NotBeNull();
            updatedProduct.ProductName.Should().Be("Updated Product");
            updatedProduct.ProductSlug.Should().Be("updated-product");
            updatedProduct.ProductPrice.Should().Be(199.99m);
        }

        [Fact]
        public async Task Delete_ShouldRemoveProductFromDb()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Product to Delete",
                ProductSlug = "product-to-delete",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId,
                ProductStatus = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Categories.AddAsync(category);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();


            _repository.Delete(product);
            await _context.SaveChangesAsync();


            var deletedProduct = await _context.Products.FindAsync(productId);
            deletedProduct.Should().BeNull();
        }

        [Fact]
        public async Task CanQueryWithDbSet()
        {

            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategorySlug = "test-category",
                IsActive = true
            };

            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Featured Product",
                    ProductSlug = "featured-product",
                    ProductPrice = 99.99m,
                    ProductStock = 10,
                    ProductSku = "TST-001",
                    CategoryId = categoryId,
                    ProductStatus = ProductStatus.Active,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Regular Product",
                    ProductSlug = "regular-product",
                    ProductPrice = 199.99m,
                    ProductStock = 20,
                    ProductSku = "TST-002",
                    CategoryId = categoryId,
                    ProductStatus = ProductStatus.Active,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Categories.AddAsync(category);
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();


            var featuredProducts = await _repository.Ts
                .Where(p => p.IsFeatured)
                .ToListAsync();


            featuredProducts.Should().NotBeNull();
            featuredProducts.Should().HaveCount(1);
            featuredProducts[0].ProductName.Should().Be("Featured Product");
        }
    }
}
