using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
        Task<List<ProductDto>> GetFeaturedProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
        Task<ProductDto> UpdateProductAsync(UpdateProductDto productDto);
        Task<bool> DeleteProductAsync(Guid id);
    }
} 