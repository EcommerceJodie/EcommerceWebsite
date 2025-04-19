using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<string> GetCategoryImagePresignedUrlAsync(Guid id, int expiryMinutes = 30);
    }
} 
