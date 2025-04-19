using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Exceptions;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Core.Validators.ProductValidators;
using Ecommerce.Services.Implementations;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<CreateProductDto>> _mockCreateValidator;
        private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateValidator;
        private readonly Mock<IMinioService> _mockMinioService;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCreateValidator = new Mock<IValidator<CreateProductDto>>();
            _mockUpdateValidator = new Mock<IValidator<UpdateProductDto>>();
            _mockMinioService = new Mock<IMinioService>();

            _productService = new ProductService(
                _mockProductRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockMinioService.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllActiveProducts()
        {

            var productsQueryable = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 3", ProductStatus = ProductStatus.Inactive, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 4", ProductStatus = ProductStatus.Active, IsDeleted = true }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Product>>();
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(productsQueryable.Provider);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productsQueryable.Expression);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productsQueryable.ElementType);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => productsQueryable.GetEnumerator());

            _mockProductRepository.Setup(x => x.Ts).Returns(mockDbSet.Object);

            var expectedProducts = new List<ProductDto>
            {
                new ProductDto { Id = productsQueryable.ElementAt(0).Id, ProductName = "Product 1" },
                new ProductDto { Id = productsQueryable.ElementAt(1).Id, ProductName = "Product 2" }
            };

            _mockMapper.Setup(x => x.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(expectedProducts);


            var result = await _productService.GetAllProductsAsync();


            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedProducts);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Test Product",
                ProductStatus = ProductStatus.Active,
                IsDeleted = false
            };

            var expectedProductDto = new ProductDto
            {
                Id = productId,
                ProductName = "Test Product"
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockMapper.Setup(x => x.Map<ProductDto>(product))
                .Returns(expectedProductDto);


            var result = await _productService.GetProductByIdAsync(productId);


            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedProductDto);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var productId = Guid.NewGuid();
            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _productService.GetProductByIdAsync(productId));
        }

        [Fact]
        public async Task GetProductByIdAsync_WithDeletedProduct_ShouldThrowEntityNotFoundException()
        {

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Test Product",
                ProductStatus = ProductStatus.Active,
                IsDeleted = true
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _productService.GetProductByIdAsync(productId));
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldCreateProduct()
        {

            var categoryId = Guid.NewGuid();
            var createProductDto = new CreateProductDto
            {
                ProductName = "New Product",
                ProductDescription = "Test Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId
            };

            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                IsDeleted = false
            };

            var product = new Product
            {
                ProductName = "New Product",
                ProductDescription = "Test Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId,
                ProductStatus = ProductStatus.Active
            };

            var expectedProductDto = new ProductDto
            {
                Id = Guid.NewGuid(),
                ProductName = "New Product",
                ProductDescription = "Test Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId
            };

            _mockCreateValidator.Setup(x => x.ValidateAsync(createProductDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockMapper.Setup(x => x.Map<Product>(createProductDto))
                .Returns(product);

            _mockMapper.Setup(x => x.Map<ProductDto>(product))
                .Returns(expectedProductDto);

            _mockUnitOfWork.Setup(x => x.HasActiveTransaction())
                .Returns(false);


            var result = await _productService.CreateProductAsync(createProductDto);


            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedProductDto);
            
            _mockProductRepository.Verify(x => x.Add(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithInvalidData_ShouldThrowValidationException()
        {

            var createProductDto = new CreateProductDto();
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("ProductName", "Product name is required")
            };

            _mockCreateValidator.Setup(x => x.ValidateAsync(createProductDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));


            await Assert.ThrowsAsync<Ecommerce.Core.Exceptions.ValidationException>(() => 
                _productService.CreateProductAsync(createProductDto));
        }

        [Fact]
        public async Task CreateProductAsync_WithInvalidCategoryId_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            var createProductDto = new CreateProductDto
            {
                ProductName = "New Product",
                ProductDescription = "Test Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = categoryId
            };

            _mockCreateValidator.Setup(x => x.ValidateAsync(createProductDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _productService.CreateProductAsync(createProductDto));
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidData_ShouldUpdateProduct()
        {

            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description",
                ProductSlug = "updated-product",
                ProductPrice = 199.99m,
                ProductStock = 20,
                ProductSku = "TST-002",
                CategoryId = categoryId
            };

            var existingProduct = new Product
            {
                Id = productId,
                ProductName = "Old Product",
                ProductDescription = "Old Description",
                ProductSlug = "old-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = Guid.NewGuid(),
                ProductStatus = ProductStatus.Active,
                IsDeleted = false
            };

            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                IsDeleted = false
            };

            var expectedProductDto = new ProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description",
                ProductSlug = "updated-product",
                ProductPrice = 199.99m,
                ProductStock = 20,
                ProductSku = "TST-002",
                CategoryId = categoryId
            };

            _mockUpdateValidator.Setup(x => x.ValidateAsync(updateProductDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockMapper.Setup(x => x.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(expectedProductDto);

            _mockUnitOfWork.Setup(x => x.HasActiveTransaction())
                .Returns(false);


            var result = await _productService.UpdateProductAsync(updateProductDto);


            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedProductDto);
            
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WithValidId_ShouldSetIsDeletedToTrue()
        {

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Test Product",
                ProductStatus = ProductStatus.Active,
                IsDeleted = false
            };

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockUnitOfWork.Setup(x => x.HasActiveTransaction())
                .Returns(false);


            var result = await _productService.DeleteProductAsync(productId);


            result.Should().BeTrue();
            product.IsDeleted.Should().BeTrue();
            
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var productId = Guid.NewGuid();
            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _productService.DeleteProductAsync(productId));
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_WithValidCategoryId_ShouldReturnFilteredProducts()
        {

            var categoryId = Guid.NewGuid();
            
            var productsQueryable = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", CategoryId = categoryId, ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", CategoryId = categoryId, ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 3", CategoryId = Guid.NewGuid(), ProductStatus = ProductStatus.Active, IsDeleted = false }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Product>>();
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(productsQueryable.Provider);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productsQueryable.Expression);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productsQueryable.ElementType);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => productsQueryable.GetEnumerator());

            _mockProductRepository.Setup(x => x.Ts).Returns(mockDbSet.Object);

            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                IsDeleted = false
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            var expectedProducts = new List<ProductDto>
            {
                new ProductDto { Id = productsQueryable.ElementAt(0).Id, ProductName = "Product 1", CategoryId = categoryId },
                new ProductDto { Id = productsQueryable.ElementAt(1).Id, ProductName = "Product 2", CategoryId = categoryId }
            };

            _mockMapper.Setup(x => x.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(expectedProducts);


            var result = await _productService.GetProductsByCategoryAsync(categoryId);


            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedProducts);
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_WithInvalidCategoryId_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            
            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _productService.GetProductsByCategoryAsync(categoryId));
        }

        [Fact]
        public async Task GetFeaturedProductsAsync_ShouldReturnFeaturedProducts()
        {

            var productsQueryable = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", IsFeatured = true, ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", IsFeatured = true, ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 3", IsFeatured = false, ProductStatus = ProductStatus.Active, IsDeleted = false },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 4", IsFeatured = true, ProductStatus = ProductStatus.Inactive, IsDeleted = false }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Product>>();
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(productsQueryable.Provider);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(productsQueryable.Expression);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(productsQueryable.ElementType);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(() => productsQueryable.GetEnumerator());

            _mockProductRepository.Setup(x => x.Ts).Returns(mockDbSet.Object);

            var expectedProducts = new List<ProductDto>
            {
                new ProductDto { Id = productsQueryable.ElementAt(0).Id, ProductName = "Product 1", IsFeatured = true },
                new ProductDto { Id = productsQueryable.ElementAt(1).Id, ProductName = "Product 2", IsFeatured = true }
            };

            _mockMapper.Setup(x => x.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(expectedProducts);


            var result = await _productService.GetFeaturedProductsAsync();


            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedProducts);
        }
    }
}
