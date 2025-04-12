using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Ecommerce.UnitTests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _controller = new ProductsController(_productServiceMock.Object);
            
            // Giả lập người dùng đã được xác thực có quyền Admin
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "UserId"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 1" },
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 2" }
            };

            _productServiceMock.Setup(service => service.GetAllProductsAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedProducts = okResult.Value as List<ProductDto>;
            returnedProducts.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkResult_WithEmptyList_WhenNoProductsExist()
        {
            // Arrange
            var emptyList = new List<ProductDto>();

            _productServiceMock.Setup(service => service.GetAllProductsAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedProducts = okResult.Value as List<ProductDto>;
            returnedProducts.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProduct_ShouldReturnOkResult_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new ProductDto
            {
                Id = productId,
                ProductName = "Test Product",
                ProductDescription = "Test Description"
            };

            _productServiceMock.Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedProduct = okResult.Value as ProductDto;
            returnedProduct.Should().BeEquivalentTo(product);
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnOkResult_WithListOfProducts()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var products = new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 1", CategoryId = categoryId },
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 2", CategoryId = categoryId }
            };

            _productServiceMock.Setup(service => service.GetProductsByCategoryAsync(categoryId))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProductsByCategory(categoryId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedProducts = okResult.Value as List<ProductDto>;
            returnedProducts.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnOkResult_WithEmptyList_WhenNoProductsExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var emptyList = new List<ProductDto>();

            _productServiceMock.Setup(service => service.GetProductsByCategoryAsync(categoryId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetProductsByCategory(categoryId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedProducts = okResult.Value as List<ProductDto>;
            returnedProducts.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateProduct_ShouldReturnCreatedAtAction_WhenProductIsCreated()
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

            var createdProduct = new ProductDto
            {
                Id = Guid.NewGuid(),
                ProductName = "New Product",
                ProductDescription = "Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                CreatedAt = DateTime.UtcNow
            };

            _productServiceMock.Setup(service => service.CreateProductAsync(createProductDto))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.CreateProduct(createProductDto);

            // Assert
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            createdAtActionResult.ActionName.Should().Be(nameof(_controller.GetProduct));
            createdAtActionResult.RouteValues["id"].Should().Be(createdProduct.Id);

            var returnedProduct = createdAtActionResult.Value as ProductDto;
            returnedProduct.Should().BeEquivalentTo(createdProduct);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnNoContent_WhenProductIsUpdated()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description"
            };

            var updatedProduct = new ProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description"
            };

            _productServiceMock.Setup(service => service.UpdateProductAsync(updateProductDto))
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateProduct(productId, updateProductDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = differentId,
                ProductName = "Updated Product"
            };

            // Act
            var result = await _controller.UpdateProduct(productId, updateProductDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("ID trong đường dẫn không khớp với ID trong dữ liệu");
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnNoContent_WhenProductIsDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productServiceMock.Setup(service => service.DeleteProductAsync(productId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
} 