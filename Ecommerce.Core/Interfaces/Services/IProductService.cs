using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface IProductService
    {

        Task<PagedResultDto<ProductDto>> GetPagedProductsAsync(ProductQueryDto queryDto);
        Task<PagedResultDto<ProductDto>> GetProductsByCategoryAsync(Guid categoryId, PaginationRequestDto pagination);
        

        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<List<ProductDto>> GetProductsByCategoryAsync(Guid categoryId);
        Task<List<ProductDto>> GetFeaturedProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
        Task<ProductDto> UpdateProductAsync(UpdateProductDto productDto);
        Task<bool> DeleteProductAsync(Guid id);
        Task<bool> DeleteMultipleProductsAsync(List<Guid> productIds);
        Task<ProductDto> DuplicateProductAsync(DuplicateProductDto duplicateDto);
        
        Task<ProductImageDto> AddProductImageAsync(Guid productId, AddProductImageDto imageDto);
        Task<bool> DeleteProductImageAsync(Guid imageId);
        Task<bool> SetMainProductImageAsync(Guid imageId);
        

        Task<string> GetProductImagePresignedUrlAsync(Guid imageId, int expiryMinutes = 30);
        

        Task<string> GetProductMainImagePresignedUrlAsync(Guid productId, int expiryMinutes = 30);
    }
} 
