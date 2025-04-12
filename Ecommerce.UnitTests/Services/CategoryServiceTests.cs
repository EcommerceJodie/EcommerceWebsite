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
    // Mock class for CategoryService that overrides EF Core specific methods
    public class TestableCategoryService : CategoryService
    {
        public TestableCategoryService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, mapper)
        {
        }

        protected override Task<List<Category>> GetCategoriesAsync(IQueryable<Category> query)
        {
            return Task.FromResult(query.ToList());
        }
    }

    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryRepositoryMock = new Mock<IRepository<Category>>();
            _mapperMock = new Mock<IMapper>();

            _unitOfWorkMock.Setup(uow => uow.Repository<Category>())
                .Returns(_categoryRepositoryMock.Object);

            _categoryService = new TestableCategoryService(_unitOfWorkMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllActiveCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), CategoryName = "Category 1", IsDeleted = false, DisplayOrder = 1 },
                new Category { Id = Guid.NewGuid(), CategoryName = "Category 2", IsDeleted = false, DisplayOrder = 2 },
                new Category { Id = Guid.NewGuid(), CategoryName = "Category 3", IsDeleted = true, DisplayOrder = 3 }
            };

            var categoryDtos = new List<CategoryDto>
            {
                new CategoryDto { Id = categories[0].Id, CategoryName = "Category 1" },
                new CategoryDto { Id = categories[1].Id, CategoryName = "Category 2" }
            };

            _categoryRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(categories.AsQueryable());

            _mapperMock.Setup(mapper => mapper.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
                .Returns(categoryDtos);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(categoryDtos);
            result.Count.Should().Be(2); // Only non-deleted categories
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnEmptyList_WhenNoCategoriesExist()
        {
            // Arrange
            var emptyList = new List<Category>();
            var emptyDtoList = new List<CategoryDto>();

            _categoryRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(emptyList.AsQueryable());

            _mapperMock.Setup(mapper => mapper.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
                .Returns(emptyDtoList);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category { Id = categoryId, CategoryName = "Test Category", IsDeleted = false };
            var categoryDto = new CategoryDto { Id = categoryId, CategoryName = "Test Category" };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mapperMock.Setup(mapper => mapper.Map<CategoryDto>(It.IsAny<Category>()))
                .Returns(categoryDto);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(categoryDto);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _categoryService.GetCategoryByIdAsync(categoryId));
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowArgumentNullException_WhenCategoryDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _categoryService.CreateCategoryAsync(null));
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldReturnNewCategory_WhenSuccessful()
        {
            // Arrange
            var createCategoryDto = new CreateCategoryDto
            {
                CategoryName = "New Category",
                CategoryDescription = "Description",
                CategorySlug = "new-category"
            };

            var newCategory = new Category
            {
                Id = Guid.NewGuid(),
                CategoryName = "New Category",
                CategoryDescription = "Description",
                CategorySlug = "new-category"
            };

            var categoryDto = new CategoryDto
            {
                Id = newCategory.Id,
                CategoryName = "New Category",
                CategoryDescription = "Description",
                CategorySlug = "new-category"
            };

            _mapperMock.Setup(mapper => mapper.Map<Category>(createCategoryDto))
                .Returns(newCategory);

            _mapperMock.Setup(mapper => mapper.Map<CategoryDto>(newCategory))
                .Returns(categoryDto);

            // Act
            var result = await _categoryService.CreateCategoryAsync(createCategoryDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(categoryDto);

            _categoryRepositoryMock.Verify(repo => repo.AddAsync(newCategory), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowArgumentNullException_WhenCategoryDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _categoryService.UpdateCategoryAsync(null));
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category"
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _categoryService.UpdateCategoryAsync(updateCategoryDto));
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryIsDeleted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category"
            };

            var deletedCategory = new Category
            {
                Id = categoryId,
                CategoryName = "Original Category",
                IsDeleted = true
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(deletedCategory);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _categoryService.UpdateCategoryAsync(updateCategoryDto));
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReturnUpdatedCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category"
            };

            var existingCategory = new Category
            {
                Id = categoryId,
                CategoryName = "Original Category",
                CategoryDescription = "Original Description",
                CategorySlug = "original-category",
                IsDeleted = false
            };

            var updatedCategoryDto = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category"
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mapperMock.Setup(mapper => mapper.Map(updateCategoryDto, existingCategory))
                .Callback<UpdateCategoryDto, Category>((src, dest) =>
                {
                    dest.CategoryName = src.CategoryName;
                    dest.CategoryDescription = src.CategoryDescription;
                    dest.CategorySlug = src.CategorySlug;
                });

            _mapperMock.Setup(mapper => mapper.Map<CategoryDto>(It.IsAny<Category>()))
                .Returns(updatedCategoryDto);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(updateCategoryDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedCategoryDto);

            _categoryRepositoryMock.Verify(repo => repo.Update(existingCategory), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _categoryService.DeleteCategoryAsync(categoryId));
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowKeyNotFoundException_WhenCategoryIsDeleted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Category to Delete",
                IsDeleted = true
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _categoryService.DeleteCategoryAsync(categoryId));
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldReturnTrue_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Category to Delete",
                IsDeleted = false
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.DeleteCategoryAsync(categoryId);

            // Assert
            result.Should().BeTrue();
            category.IsDeleted.Should().BeTrue();
            category.UpdatedAt.Should().NotBeNull();

            _categoryRepositoryMock.Verify(repo => repo.Update(category), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
} 