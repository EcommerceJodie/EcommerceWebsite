using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync();
        Task<IEnumerable<Product>> GetProductsByStatusAsync(ProductStatus status);
        Task<Product> GetProductWithRelatedAsync(Guid id);
        Task<Product> GetProductBySlugAsync(string slug);
        Task<bool> IsProductSlugUniqueAsync(string slug, Guid? excludeId = null);
        Task<bool> IsProductSkuUniqueAsync(string sku, Guid? excludeId = null);
    }
} 