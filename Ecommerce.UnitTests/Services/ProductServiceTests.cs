using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.Services
{
    // Mock class for ProductService that overrides EF Core specific methods
    public class TestableProductService : ProductService
    {
        public TestableProductService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, mapper)
        {
        }

        protected override Task<List<Product>> GetProductsAsync(IQueryable<Product> query)
        {
            return Task.FromResult(query.ToList());
        }
    }

    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepository<Product>> _productRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private ProductService _productService;

        public ProductServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productRepositoryMock = new Mock<IRepository<Product>>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(uow => uow.Repository<Product>())
                .Returns(_productRepositoryMock.Object);

            _productService = new TestableProductService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", CategoryId = Guid.NewGuid() },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", CategoryId = Guid.NewGuid() }
            };

            var productDtos = new List<ProductDto>
            {
                new ProductDto { Id = products[0].Id, ProductName = "Product 1" },
                new ProductDto { Id = products[1].Id, ProductName = "Product 2" }
            };

            _productRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(products.AsQueryable());

            _mapperMock.Setup(mapper => mapper.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(productDtos);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(productDtos);
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnEmptyList_WhenNoProductsExist()
        {
            // Arrange
            var emptyList = new List<Product>();
            var emptyDtoList = new List<ProductDto>();

            _productRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(emptyList.AsQueryable());

            _mapperMock.Setup(mapper => mapper.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(emptyDtoList);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, ProductName = "Test Product", CategoryId = Guid.NewGuid() };
            var productDto = new ProductDto { Id = productId, ProductName = "Test Product" };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mapperMock.Setup(mapper => mapper.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(productDto);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(productDto);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _productService.GetProductByIdAsync(productId));
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_ShouldReturnProductsInCategory()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", CategoryId = categoryId },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", CategoryId = categoryId },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 3", CategoryId = Guid.NewGuid() }
            };

            var productDtos = new List<ProductDto>
            {
                new ProductDto { Id = products[0].Id, ProductName = "Product 1", CategoryId = categoryId },
                new ProductDto { Id = products[1].Id, ProductName = "Product 2", CategoryId = categoryId }
            };

            _productRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(products.AsQueryable());

            _mapperMock.Setup(mapper => mapper.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(productDtos);

            // Act
            var result = await _productService.GetProductsByCategoryAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(productDtos);
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetFeaturedProductsAsync_ShouldReturnFeaturedProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", IsFeatured = true },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", IsFeatured = true },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 3", IsFeatured = false }
            };

            var featuredProductDtos = new List<ProductDto>
            {
                new ProductDto { Id = products[0].Id, ProductName = "Product 1" },
                new ProductDto { Id = products[1].Id, ProductName = "Product 2" }
            };

            _productRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(products.AsQueryable());

            _mapperMock.Setup(mapper => mapper.Map<List<ProductDto>>(It.IsAny<List<Product>>()))
                .Returns(featuredProductDtos);

            // Act
            var result = await _productService.GetFeaturedProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(featuredProductDtos);
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldThrowArgumentNullException_WhenProductDtoIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _productService.CreateProductAsync(null));
        }

        [Fact]
        public async Task CreateProductAsync_ShouldReturnCreatedProduct()
        {
            // Arrange
            var createProductDto = new CreateProductDto
            {
                ProductName = "New Product",
                ProductDescription = "Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                CategoryId = Guid.NewGuid()
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "New Product",
                ProductDescription = "Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m
            };

            var productDto = new ProductDto
            {
                Id = product.Id,
                ProductName = "New Product",
                ProductDescription = "Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m
            };

            _mapperMock.Setup(mapper => mapper.Map<Product>(createProductDto))
                .Returns(product);

            _mapperMock.Setup(mapper => mapper.Map<ProductDto>(product))
                .Returns(productDto);

            // Act
            var result = await _productService.CreateProductAsync(createProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(productDto);

            _productRepositoryMock.Verify(repo => repo.AddAsync(product), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowArgumentNullException_WhenProductDtoIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _productService.UpdateProductAsync(null));
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = productId,
                ProductName = "Updated Product"
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _productService.UpdateProductAsync(updateProductDto));
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnUpdatedProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description"
            };

            var existingProduct = new Product
            {
                Id = productId,
                ProductName = "Original Product",
                ProductDescription = "Original Description"
            };

            var updatedProductDto = new ProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description"
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mapperMock.Setup(mapper => mapper.Map(updateProductDto, existingProduct))
                .Callback<UpdateProductDto, Product>((src, dest) =>
                {
                    dest.ProductName = src.ProductName;
                    dest.ProductDescription = src.ProductDescription;
                });

            _mapperMock.Setup(mapper => mapper.Map<ProductDto>(It.IsAny<Product>()))
                .Returns(updatedProductDto);

            // Act
            var result = await _productService.UpdateProductAsync(updateProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedProductDto);

            _productRepositoryMock.Verify(repo => repo.Update(existingProduct), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _productService.DeleteProductAsync(productId));
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnTrue_WhenProductIsDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                ProductName = "Product to Delete"
            };

            _productRepositoryMock.Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeTrue();

            _productRepositoryMock.Verify(repo => repo.Delete(product), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
} 