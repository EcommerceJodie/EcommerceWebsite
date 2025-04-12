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
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _controller = new CategoriesController(_categoryServiceMock.Object);
            
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
        public async Task GetCategories_ShouldReturnOkResult_WithListOfCategories()
        {
            // Arrange
            var categories = new List<CategoryDto>
            {
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 1" },
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 2" }
            };

            _categoryServiceMock.Setup(service => service.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _controller.GetCategories();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedCategories = okResult.Value as List<CategoryDto>;
            returnedCategories.Should().BeEquivalentTo(categories);
        }

        [Fact]
        public async Task GetCategories_ShouldReturnOkResult_WithEmptyList_WhenNoCategoriesExist()
        {
            // Arrange
            var emptyList = new List<CategoryDto>();

            _categoryServiceMock.Setup(service => service.GetAllCategoriesAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetCategories();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedCategories = okResult.Value as List<CategoryDto>;
            returnedCategories.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCategory_ShouldReturnOkResult_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategoryDescription = "Test Description"
            };

            _categoryServiceMock.Setup(service => service.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _controller.GetCategory(categoryId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var returnedCategory = okResult.Value as CategoryDto;
            returnedCategory.Should().BeEquivalentTo(category);
        }

        [Fact]
        public async Task CreateCategory_ShouldReturnCreatedAtAction_WhenCategoryIsCreated()
        {
            // Arrange
            var createCategoryDto = new CreateCategoryDto
            {
                CategoryName = "New Category",
                CategoryDescription = "Description",
                CategorySlug = "new-category"
            };

            var createdCategory = new CategoryDto
            {
                Id = Guid.NewGuid(),
                CategoryName = "New Category",
                CategoryDescription = "Description",
                CategorySlug = "new-category",
                CreatedAt = DateTime.UtcNow
            };

            _categoryServiceMock.Setup(service => service.CreateCategoryAsync(createCategoryDto))
                .ReturnsAsync(createdCategory);

            // Act
            var result = await _controller.CreateCategory(createCategoryDto);

            // Assert
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            createdAtActionResult.ActionName.Should().Be(nameof(_controller.GetCategory));
            createdAtActionResult.RouteValues["id"].Should().Be(createdCategory.Id);

            var returnedCategory = createdAtActionResult.Value as CategoryDto;
            returnedCategory.Should().BeEquivalentTo(createdCategory);
        }

        [Fact]
        public async Task UpdateCategory_ShouldReturnNoContent_WhenCategoryIsUpdated()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description"
            };

            var updatedCategory = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description"
            };

            _categoryServiceMock.Setup(service => service.UpdateCategoryAsync(updateCategoryDto))
                .ReturnsAsync(updatedCategory);

            // Act
            var result = await _controller.UpdateCategory(categoryId, updateCategoryDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateCategory_ShouldReturnBadRequest_WhenIdsMismatch()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = differentId,
                CategoryName = "Updated Category"
            };

            // Act
            var result = await _controller.UpdateCategory(categoryId, updateCategoryDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("ID trong đường dẫn không khớp với ID trong dữ liệu");
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturnNoContent_WhenCategoryIsDeleted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryServiceMock.Setup(service => service.DeleteCategoryAsync(categoryId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
} 