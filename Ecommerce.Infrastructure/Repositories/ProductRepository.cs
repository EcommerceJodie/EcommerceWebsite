using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Product> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsFeatured && !p.IsDeleted)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByStatusAsync(ProductStatus status)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.ProductStatus == status && !p.IsDeleted)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<Product> GetProductWithRelatedAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductRatings)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Product> GetProductBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductSlug == slug && !p.IsDeleted);
        }

        public async Task<bool> IsProductSlugUniqueAsync(string slug, Guid? excludeId = null)
        {
            var query = _context.Products
                .Where(p => p.ProductSlug == slug && !p.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsProductSkuUniqueAsync(string sku, Guid? excludeId = null)
        {
            var query = _context.Products
                .Where(p => p.ProductSku == sku && !p.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
} 
