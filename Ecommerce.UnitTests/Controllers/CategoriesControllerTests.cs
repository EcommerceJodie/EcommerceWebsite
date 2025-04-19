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
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoriesController _controller;

        public CategoriesControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoriesController(_mockCategoryService.Object);
        }

        [Fact]
        public async Task GetCategories_ShouldReturnOkResult_WithListOfCategories()
        {

            var categories = new List<CategoryDto>
            {
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 1" },
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 2" }
            };

            _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
                .ReturnsAsync(categories);


            var result = await _controller.GetCategories();


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedCategories = okResult.Value as IEnumerable<CategoryDto>;
            returnedCategories.Should().BeEquivalentTo(categories);
        }

        [Fact]
        public async Task GetActiveCategories_ShouldReturnOkResult_WithActiveCategories()
        {

            var categories = new List<CategoryDto>
            {
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 1", IsActive = true },
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 2", IsActive = true },
                new CategoryDto { Id = Guid.NewGuid(), CategoryName = "Category 3", IsActive = false }
            };

            _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
                .ReturnsAsync(categories);


            var result = await _controller.GetActiveCategories();


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedCategories = okResult.Value as IEnumerable<CategoryDto>;
            returnedCategories.Should().HaveCount(2);
            returnedCategories.Should().OnlyContain(c => c.IsActive);
        }

        [Fact]
        public async Task GetCategory_WithValidId_ShouldReturnOkResult_WithCategory()
        {

            var categoryId = Guid.NewGuid();
            var category = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategoryDescription = "Test Description",
                CategorySlug = "test-category",
                DisplayOrder = 1,
                IsActive = true
            };

            _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);


            var result = await _controller.GetCategory(categoryId);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedCategory = okResult.Value as CategoryDto;
            returnedCategory.Should().BeEquivalentTo(category);
        }

        [Fact]
        public async Task GetCategory_WithInvalidId_ShouldReturnNotFound()
        {

            var categoryId = Guid.NewGuid();
            
            _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(categoryId))
                .ThrowsAsync(new EntityNotFoundException("Danh mục", categoryId));


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _controller.GetCategory(categoryId));
        }

        [Fact]
        public async Task GetCategoryImagePresignedUrl_WithValidId_ShouldReturnOkResult_WithUrl()
        {

            var categoryId = Guid.NewGuid();
            var presignedUrl = "https:

            _mockCategoryService.Setup(x => x.GetCategoryImagePresignedUrlAsync(categoryId, 30))
                .ReturnsAsync(presignedUrl);


            var result = await _controller.GetCategoryImagePresignedUrl(categoryId);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

            var returnedObject = okResult.Value as dynamic;
            string returnedUrl = returnedObject.url;
            returnedUrl.Should().Be(presignedUrl);
        }

        [Fact]
        public async Task CreateCategory_WithValidData_ShouldReturnCreatedAtResult_WithCreatedCategory()
        {

            var createCategoryDto = new CreateCategoryDto
            {
                CategoryName = "New Category",
                CategoryDescription = "New Description",
                CategorySlug = "new-category",
                DisplayOrder = 1,
                IsActive = true
            };

            var createdCategory = new CategoryDto
            {
                Id = Guid.NewGuid(),
                CategoryName = "New Category",
                CategoryDescription = "New Description",
                CategorySlug = "new-category",
                DisplayOrder = 1,
                IsActive = true
            };

            _mockCategoryService.Setup(x => x.CreateCategoryAsync(createCategoryDto))
                .ReturnsAsync(createdCategory);


            var result = await _controller.CreateCategory(createCategoryDto);


            var createdAtResult = result.Result as CreatedAtActionResult;
            createdAtResult.Should().NotBeNull();
            createdAtResult.StatusCode.Should().Be(StatusCodes.Status201Created);
            createdAtResult.ActionName.Should().Be(nameof(CategoriesController.GetCategory));
            createdAtResult.RouteValues["id"].Should().Be(createdCategory.Id);

            var returnedCategory = createdAtResult.Value as CategoryDto;
            returnedCategory.Should().BeEquivalentTo(createdCategory);
        }

        [Fact]
        public async Task CreateCategory_WithInvalidData_ShouldReturnBadRequest()
        {

            var createCategoryDto = new CreateCategoryDto();
            
            _controller.ModelState.AddModelError("CategoryName", "Category name is required");


            var result = await _controller.CreateCategory(createCategoryDto);


            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task UpdateCategory_WithValidData_ShouldReturnNoContent()
        {

            var categoryId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category",
                DisplayOrder = 2,
                IsActive = true
            };

            var updatedCategory = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category",
                DisplayOrder = 2,
                IsActive = true
            };

            _mockCategoryService.Setup(x => x.UpdateCategoryAsync(updateCategoryDto))
                .ReturnsAsync(updatedCategory);


            var result = await _controller.UpdateCategory(categoryId, updateCategoryDto);


            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task UpdateCategory_WithMismatchedIds_ShouldReturnBadRequest()
        {

            var categoryId = Guid.NewGuid();
            var differentId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = differentId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category",
                DisplayOrder = 2,
                IsActive = true
            };


            var result = await _controller.UpdateCategory(categoryId, updateCategoryDto);


            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task DeleteCategory_WithValidId_ShouldReturnNoContent()
        {

            var categoryId = Guid.NewGuid();
            
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                .ReturnsAsync(true);


            var result = await _controller.DeleteCategory(categoryId);


            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }

        [Fact]
        public async Task DeleteCategory_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            
            _mockCategoryService.Setup(x => x.DeleteCategoryAsync(categoryId))
                .ThrowsAsync(new EntityNotFoundException("Danh mục", categoryId));


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _controller.DeleteCategory(categoryId));
        }
    }
}
