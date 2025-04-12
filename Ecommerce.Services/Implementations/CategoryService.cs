using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Exceptions;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Validators.CategoryValidators;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateCategoryDto> _createValidator;
        private readonly IValidator<UpdateCategoryDto> _updateValidator;
        private readonly IMinioService _minioService;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateCategoryDto> createValidator,
            IValidator<UpdateCategoryDto> updateValidator,
            IMinioService minioService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _minioService = minioService;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.Ts
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", id);
            }

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            var validationResult = await _createValidator.ValidateAsync(categoryDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Ecommerce.Core.Exceptions.ValidationException(errors);
            }

            if (categoryDto.CategoryImage != null)
            {
                try
                {
                    var fileName = $"categories/{Guid.NewGuid()}{System.IO.Path.GetExtension(categoryDto.CategoryImage.FileName)}";
                    var imageUrl = await _minioService.UploadFileAsync(categoryDto.CategoryImage, fileName);
                    categoryDto.CategoryImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi khi tải lên hình ảnh: {ex.Message}");
                }
            }
            else if (string.IsNullOrEmpty(categoryDto.CategoryImageUrl))
            {
                categoryDto.CategoryImageUrl = ""; 
            }

            var category = _mapper.Map<Category>(categoryDto);
            
            category.Id = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _categoryRepository.Add(category);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto categoryDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(categoryDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new Ecommerce.Core.Exceptions.ValidationException(errors);
            }

            var category = await _categoryRepository.GetByIdAsync(categoryDto.Id);
            
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", categoryDto.Id);
            }

            if (categoryDto.CategoryImage != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(category.CategoryImageUrl))
                    {
                        var oldImagePath = ExtractObjectNameFromUrl(category.CategoryImageUrl);
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            await _minioService.RemoveFileAsync(oldImagePath);
                        }
                    }

                    var fileName = $"categories/{Guid.NewGuid()}{System.IO.Path.GetExtension(categoryDto.CategoryImage.FileName)}";
                    var imageUrl = await _minioService.UploadFileAsync(categoryDto.CategoryImage, fileName);
                    categoryDto.CategoryImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi khi tải lên hình ảnh: {ex.Message}");
                }
            }
            else if (string.IsNullOrEmpty(categoryDto.CategoryImageUrl))
            {
                categoryDto.CategoryImageUrl = category.CategoryImageUrl ?? "";
            }

            _mapper.Map(categoryDto, category);
            
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _categoryRepository.Update(category);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", id);
            }

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _categoryRepository.Update(category);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private string ExtractObjectNameFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;

               
                var uri = new Uri(url);
                var segments = uri.Segments;

                if (segments.Length > 2)
                {
                    return string.Join("", segments.Skip(2));
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
} 