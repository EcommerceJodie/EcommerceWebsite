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

            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            

            try
            {
                foreach (var categoryDto in categoryDtos)
                {
                    if (!string.IsNullOrEmpty(categoryDto.CategoryImageUrl))
                    {
                        var objectName = ExtractObjectNameFromUrl(categoryDto.CategoryImageUrl);
                        if (!string.IsNullOrEmpty(objectName))
                        {

                            var presignedUrl = await _minioService.GeneratePresignedDownloadUrlAsync(objectName, 60);
                            categoryDto.CategoryImageUrl = presignedUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Không thể tạo presigned URL cho danh sách: {ex.Message}");
            }

            return categoryDtos;
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", id);
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            

            try {
                if (!string.IsNullOrEmpty(categoryDto.CategoryImageUrl))
                {
                    var objectName = ExtractObjectNameFromUrl(categoryDto.CategoryImageUrl);
                    if (!string.IsNullOrEmpty(objectName))
                    {

                        var presignedUrl = await _minioService.GeneratePresignedDownloadUrlAsync(objectName, 60);
                        categoryDto.CategoryImageUrl = presignedUrl;
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Không thể tạo presigned URL: {ex.Message}");
            }

            return categoryDto;
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

            bool startedTransaction = false;
            try
            {
                if (!_unitOfWork.HasActiveTransaction())
                {
                    await _unitOfWork.BeginTransactionAsync();
                    startedTransaction = true;
                }

                _categoryRepository.Add(category);
                await _unitOfWork.CompleteAsync();
                
                if (startedTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                if (startedTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto categoryDto)
        {
            try
            {
                Console.WriteLine($"UpdateCategoryAsync được gọi với ID: {categoryDto.Id}");
                Console.WriteLine($"CategoryImageUrl ban đầu: {categoryDto.CategoryImageUrl}");
                
                // Nếu URL quá dài, cắt phần tham số query
                if (!string.IsNullOrEmpty(categoryDto.CategoryImageUrl) && categoryDto.CategoryImageUrl.Length > 250 && categoryDto.CategoryImageUrl.Contains("?"))
                {
                    categoryDto.CategoryImageUrl = categoryDto.CategoryImageUrl.Substring(0, categoryDto.CategoryImageUrl.IndexOf("?"));
                    Console.WriteLine($"Đã cắt tham số query từ URL: {categoryDto.CategoryImageUrl}");
                }
                
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
                
                Console.WriteLine($"CategoryImageUrl trong database: {category.CategoryImageUrl}");

                // Chỉ xử lý hình ảnh khi có file mới
                if (categoryDto.CategoryImage != null && categoryDto.CategoryImage.Length > 0)
                {
                    Console.WriteLine("Đang xử lý file hình ảnh mới...");
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
                        Console.WriteLine($"CategoryImageUrl sau khi upload: {categoryDto.CategoryImageUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Lỗi khi xử lý hình ảnh: {ex.Message}");
                        Console.WriteLine($"StackTrace: {ex.StackTrace}");
                        throw new Exception($"Lỗi khi tải lên hình ảnh: {ex.Message}", ex);
                    }
                }
                else
                {
                    Console.WriteLine("Không có file hình ảnh mới.");
                    // Nếu không có file mới và CategoryImageUrl rỗng, sử dụng URL hiện tại
                    if (string.IsNullOrEmpty(categoryDto.CategoryImageUrl))
                    {
                        Console.WriteLine("Sử dụng lại hình ảnh hiện tại từ database");
                        categoryDto.CategoryImageUrl = category.CategoryImageUrl ?? "";
                    }
                    else
                    {
                        Console.WriteLine($"Giữ nguyên CategoryImageUrl: {categoryDto.CategoryImageUrl}");
                    }
                }

                _mapper.Map(categoryDto, category);
                
                category.UpdatedAt = DateTime.UtcNow;
                Console.WriteLine($"CategoryImageUrl sau khi mapping: {category.CategoryImageUrl}");

                bool startedTransaction = false;
                try
                {
                    if (!_unitOfWork.HasActiveTransaction())
                    {
                        await _unitOfWork.BeginTransactionAsync();
                        startedTransaction = true;
                    }

                    _categoryRepository.Update(category);
                    await _unitOfWork.CompleteAsync();
                    
                    if (startedTransaction)
                    {
                        await _unitOfWork.CommitTransactionAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi cập nhật database: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    if (startedTransaction)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                    }
                    throw;
                }

                var result = _mapper.Map<CategoryDto>(category);
                Console.WriteLine($"CategoryImageUrl trong kết quả trả về: {result.CategoryImageUrl}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nghiêm trọng trong UpdateCategoryAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
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

            bool startedTransaction = false;
            try
            {
                if (!_unitOfWork.HasActiveTransaction())
                {
                    await _unitOfWork.BeginTransactionAsync();
                    startedTransaction = true;
                }

                _categoryRepository.Update(category);
                await _unitOfWork.CompleteAsync();
                
                if (startedTransaction)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
                return true;
            }
            catch (Exception)
            {
                if (startedTransaction)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                throw;
            }
        }

        public async Task<string> GetCategoryImagePresignedUrlAsync(Guid id, int expiryMinutes = 30)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            
            if (category == null || category.IsDeleted)
            {
                throw new EntityNotFoundException("Danh mục", id);
            }
            
            if (string.IsNullOrEmpty(category.CategoryImageUrl))
            {
                throw new Exception("Danh mục này không có hình ảnh");
            }
            

            var objectName = ExtractObjectNameFromUrl(category.CategoryImageUrl);
            if (string.IsNullOrEmpty(objectName))
            {
                throw new Exception("Không thể trích xuất tên file từ URL");
            }
            

            var presignedUrl = await _minioService.GeneratePresignedDownloadUrlAsync(objectName, expiryMinutes);
            return presignedUrl;
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
