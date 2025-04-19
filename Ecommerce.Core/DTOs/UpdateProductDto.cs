using System;
using Ecommerce.Core.Models.Enums;

namespace Ecommerce.Core.DTOs
{
    public class UpdateProductDto
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
    }
} 
