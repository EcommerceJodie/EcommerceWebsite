using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Exceptions;
using Ecommerce.Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new ProductsController(_mockProductService.Object);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkResult_WithListOfProducts()
        {

            var products = new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 1" },
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 2" }
            };

            _mockProductService.Setup(x => x.GetAllProductsAsync())
                .ReturnsAsync(products);


            var result = await _controller.GetProducts();


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedProducts = okResult.Value as IEnumerable<ProductDto>;
            returnedProducts.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task GetProduct_WithValidId_ShouldReturnOkResult_WithProduct()
        {

            var productId = Guid.NewGuid();
            var product = new ProductDto
            {
                Id = productId,
                ProductName = "Test Product",
                ProductDescription = "Test Description",
                ProductSlug = "test-product",
                ProductPrice = 99.99m,
                ProductStock = 10
            };

            _mockProductService.Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);


            var result = await _controller.GetProduct(productId);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedProduct = okResult.Value as ProductDto;
            returnedProduct.Should().BeEquivalentTo(product);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var productId = Guid.NewGuid();
            
            _mockProductService.Setup(x => x.GetProductByIdAsync(productId))
                .ThrowsAsync(new EntityNotFoundException("Sản phẩm", productId));


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _controller.GetProduct(productId));
        }

        [Fact]
        public async Task GetProductImagePresignedUrl_WithValidId_ShouldReturnOkResult_WithUrl()
        {

            var productId = Guid.NewGuid();
            var imageId = 1; 
            var presignedUrl = "https:

            _mockProductService.Setup(x => x.GetProductImagePresignedUrlAsync(productId, imageId))
                .ReturnsAsync(presignedUrl);


            var result = await _controller.GetProductImagePresignedUrl(productId, imageId);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedObject = okResult.Value as dynamic;
            string returnedUrl = returnedObject.url;
            returnedUrl.Should().Be(presignedUrl);
        }

        [Fact]
        public async Task GetFeaturedProducts_ShouldReturnOkResult_WithFeaturedProducts()
        {

            var featuredProducts = new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Featured Product 1", IsFeatured = true },
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Featured Product 2", IsFeatured = true }
            };

            _mockProductService.Setup(x => x.GetFeaturedProductsAsync())
                .ReturnsAsync(featuredProducts);


            var result = await _controller.GetFeaturedProducts();


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedProducts = okResult.Value as IEnumerable<ProductDto>;
            returnedProducts.Should().BeEquivalentTo(featuredProducts);
        }

        [Fact]
        public async Task GetProductsByCategory_WithValidCategoryId_ShouldReturnOkResult_WithProducts()
        {

            var categoryId = Guid.NewGuid();
            var products = new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 1", CategoryId = categoryId },
                new ProductDto { Id = Guid.NewGuid(), ProductName = "Product 2", CategoryId = categoryId }
            };

            _mockProductService.Setup(x => x.GetProductsByCategoryAsync(categoryId))
                .ReturnsAsync(products);


            var result = await _controller.GetProductsByCategory(categoryId);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedProducts = okResult.Value as IEnumerable<ProductDto>;
            returnedProducts.Should().BeEquivalentTo(products);
        }

        [Fact]
        public async Task GetProductsByCategory_WithInvalidCategoryId_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            
            _mockProductService.Setup(x => x.GetProductsByCategoryAsync(categoryId))
                .ThrowsAsync(new EntityNotFoundException("Danh mục", categoryId));


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _controller.GetProductsByCategory(categoryId));
        }

        [Fact]
        public async Task CreateProduct_WithValidData_ShouldReturnCreatedAtResult_WithCreatedProduct()
        {

            var createProductDto = new CreateProductDto
            {
                ProductName = "New Product",
                ProductDescription = "New Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = Guid.NewGuid()
            };

            var createdProduct = new ProductDto
            {
                Id = Guid.NewGuid(),
                ProductName = "New Product",
                ProductDescription = "New Description",
                ProductSlug = "new-product",
                ProductPrice = 99.99m,
                ProductStock = 10,
                ProductSku = "TST-001",
                CategoryId = Guid.NewGuid()
            };

            _mockProductService.Setup(x => x.CreateProductAsync(createProductDto))
                .ReturnsAsync(createdProduct);


            var result = await _controller.CreateProduct(createProductDto);


            var createdAtResult = result.Result as CreatedAtActionResult;
            createdAtResult.Should().NotBeNull();
            createdAtResult.StatusCode.Should().Be(StatusCodes.Status201Created);
            createdAtResult.ActionName.Should().Be(nameof(ProductsController.GetProduct));
            createdAtResult.RouteValues["id"].Should().Be(createdProduct.Id);

            var returnedProduct = createdAtResult.Value as ProductDto;
            returnedProduct.Should().BeEquivalentTo(createdProduct);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
        {

            var createProductDto = new CreateProductDto();
            
            _controller.ModelState.AddModelError("ProductName", "Product name is required");


            var result = await _controller.CreateProduct(createProductDto);


            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task UpdateProduct_WithValidData_ShouldReturnNoContent()
        {

            var productId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description",
                ProductSlug = "updated-product",
                ProductPrice = 199.99m,
                ProductStock = 20,
                ProductSku = "TST-002",
                CategoryId = Guid.NewGuid()
            };

            var updatedProduct = new ProductDto
            {
                Id = productId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description",
                ProductSlug = "updated-product",
                ProductPrice = 199.99m,
                ProductStock = 20,
                ProductSku = "TST-002",
                CategoryId = Guid.NewGuid()
            };

            _mockProductService.Setup(x => x.UpdateProductAsync(updateProductDto))
                .ReturnsAsync(updatedProduct);


            var result = await _controller.UpdateProduct(productId, updateProductDto);


            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task UpdateProduct_WithMismatchedIds_ShouldReturnBadRequest()
        {

            var productId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var updateProductDto = new UpdateProductDto
            {
                Id = differentId,
                ProductName = "Updated Product",
                ProductDescription = "Updated Description",
                ProductSlug = "updated-product",
                ProductPrice = 199.99m,
                ProductStock = 20,
                ProductSku = "TST-002",
                CategoryId = Guid.NewGuid()
            };


            var result = await _controller.UpdateProduct(productId, updateProductDto);


            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_ShouldReturnNoContent()
        {

            var productId = Guid.NewGuid();
            
            _mockProductService.Setup(x => x.DeleteProductAsync(productId))
                .ReturnsAsync(true);


            var result = await _controller.DeleteProduct(productId);


            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var productId = Guid.NewGuid();
            
            _mockProductService.Setup(x => x.DeleteProductAsync(productId))
                .ThrowsAsync(new EntityNotFoundException("Sản phẩm", productId));


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _controller.DeleteProduct(productId));
        }
    }
}
