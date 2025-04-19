using System;
using System.Collections.Generic;
using Ecommerce.Core.Models.Enums;

namespace Ecommerce.Core.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductSlug { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductDiscountPrice { get; set; }
        public int ProductStock { get; set; }
        public string ProductSku { get; set; }
        public ProductStatus ProductStatus { get; set; }
        public bool IsFeatured { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<ProductImageDto> ImageUrlsWithDetails { get; set; } = new List<ProductImageDto>();
        

        public string StockStatus => ProductStock <= 0 ? "Hết hàng" : (ProductStock <= 5 ? "Sắp hết hàng" : "Còn hàng");
        public int RatingCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0;
        public decimal OriginalPrice => ProductPrice;
        public decimal DisplayPrice => ProductDiscountPrice ?? ProductPrice;
        public int DiscountPercentage => ProductDiscountPrice.HasValue
            ? (int)Math.Round((1 - (decimal)ProductDiscountPrice.Value / ProductPrice) * 100)
            : 0;
    }
} 
