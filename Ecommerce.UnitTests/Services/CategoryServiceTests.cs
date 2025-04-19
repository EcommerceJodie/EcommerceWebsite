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
using Ecommerce.Core.Validators.CategoryValidators;
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
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<CreateCategoryDto>> _mockCreateValidator;
        private readonly Mock<IValidator<UpdateCategoryDto>> _mockUpdateValidator;
        private readonly Mock<IMinioService> _mockMinioService;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockCreateValidator = new Mock<IValidator<CreateCategoryDto>>();
            _mockUpdateValidator = new Mock<IValidator<UpdateCategoryDto>>();
            _mockMinioService = new Mock<IMinioService>();

            _categoryService = new CategoryService(
                _mockCategoryRepository.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object,
                _mockMinioService.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllActiveCategories()
        {

            var categoriesQueryable = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), CategoryName = "Category 1", IsDeleted = false, DisplayOrder = 1 },
                new Category { Id = Guid.NewGuid(), CategoryName = "Category 2", IsDeleted = false, DisplayOrder = 2 },
                new Category { Id = Guid.NewGuid(), CategoryName = "Category 3", IsDeleted = true, DisplayOrder = 3 }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Category>>();
            mockDbSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(categoriesQueryable.Provider);
            mockDbSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(categoriesQueryable.Expression);
            mockDbSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(categoriesQueryable.ElementType);
            mockDbSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(() => categoriesQueryable.GetEnumerator());

            _mockCategoryRepository.Setup(x => x.Ts).Returns(mockDbSet.Object);

            var expectedCategories = new List<CategoryDto>
            {
                new CategoryDto { Id = categoriesQueryable.ElementAt(0).Id, CategoryName = "Category 1", CategoryImageUrl = null },
                new CategoryDto { Id = categoriesQueryable.ElementAt(1).Id, CategoryName = "Category 2", CategoryImageUrl = null },
            };

            _mockMapper.Setup(x => x.Map<List<CategoryDto>>(It.IsAny<List<Category>>()))
                .Returns(expectedCategories);


            var result = await _categoryService.GetAllCategoriesAsync();


            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedCategories);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithValidId_ShouldReturnCategory()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                IsDeleted = false
            };

            var expectedCategoryDto = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Test Category",
                CategoryImageUrl = null
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockMapper.Setup(x => x.Map<CategoryDto>(category))
                .Returns(expectedCategoryDto);


            var result = await _categoryService.GetCategoryByIdAsync(categoryId);


            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedCategoryDto);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _categoryService.GetCategoryByIdAsync(categoryId));
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WithDeletedCategory_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                IsDeleted = true
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _categoryService.GetCategoryByIdAsync(categoryId));
        }

        [Fact]
        public async Task CreateCategoryAsync_WithValidData_ShouldCreateCategory()
        {

            var createCategoryDto = new CreateCategoryDto
            {
                CategoryName = "New Category",
                CategoryDescription = "Test Description",
                CategorySlug = "new-category",
                DisplayOrder = 1,
                IsActive = true
            };

            var category = new Category
            {
                CategoryName = "New Category",
                CategoryDescription = "Test Description",
                CategorySlug = "new-category",
                DisplayOrder = 1,
                IsActive = true
            };

            var expectedCategoryDto = new CategoryDto
            {
                Id = Guid.NewGuid(),
                CategoryName = "New Category",
                CategoryDescription = "Test Description",
                CategorySlug = "new-category",
                DisplayOrder = 1,
                IsActive = true
            };

            _mockCreateValidator.Setup(x => x.ValidateAsync(createCategoryDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMapper.Setup(x => x.Map<Category>(createCategoryDto))
                .Returns(category);

            _mockMapper.Setup(x => x.Map<CategoryDto>(category))
                .Returns(expectedCategoryDto);

            _mockUnitOfWork.Setup(x => x.HasActiveTransaction())
                .Returns(false);


            var result = await _categoryService.CreateCategoryAsync(createCategoryDto);


            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedCategoryDto);
            
            _mockCategoryRepository.Verify(x => x.Add(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_WithInvalidData_ShouldThrowValidationException()
        {

            var createCategoryDto = new CreateCategoryDto();
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("CategoryName", "Category name is required")
            };

            _mockCreateValidator.Setup(x => x.ValidateAsync(createCategoryDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));


            await Assert.ThrowsAsync<Ecommerce.Core.Exceptions.ValidationException>(() => 
                _categoryService.CreateCategoryAsync(createCategoryDto));
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithValidData_ShouldUpdateCategory()
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

            var existingCategory = new Category
            {
                Id = categoryId,
                CategoryName = "Old Category",
                CategoryDescription = "Old Description",
                CategorySlug = "old-category",
                DisplayOrder = 1,
                IsActive = true,
                IsDeleted = false
            };

            var updatedCategory = new Category
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category",
                DisplayOrder = 2,
                IsActive = true,
                IsDeleted = false
            };

            var expectedCategoryDto = new CategoryDto
            {
                Id = categoryId,
                CategoryName = "Updated Category",
                CategoryDescription = "Updated Description",
                CategorySlug = "updated-category",
                DisplayOrder = 2,
                IsActive = true
            };

            _mockUpdateValidator.Setup(x => x.ValidateAsync(updateCategoryDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockMapper.Setup(x => x.Map<CategoryDto>(It.IsAny<Category>()))
                .Returns(expectedCategoryDto);

            _mockUnitOfWork.Setup(x => x.HasActiveTransaction())
                .Returns(false);


            var result = await _categoryService.UpdateCategoryAsync(updateCategoryDto);


            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedCategoryDto);
            
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithValidId_ShouldSetIsDeletedToTrue()
        {

            var categoryId = Guid.NewGuid();
            var category = new Category
            {
                Id = categoryId,
                CategoryName = "Test Category",
                IsDeleted = false
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockUnitOfWork.Setup(x => x.HasActiveTransaction())
                .Returns(false);


            var result = await _categoryService.DeleteCategoryAsync(categoryId);


            result.Should().BeTrue();
            category.IsDeleted.Should().BeTrue();
            
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
        {

            var categoryId = Guid.NewGuid();
            _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);


            await Assert.ThrowsAsync<EntityNotFoundException>(() => 
                _categoryService.DeleteCategoryAsync(categoryId));
        }
    }
}
