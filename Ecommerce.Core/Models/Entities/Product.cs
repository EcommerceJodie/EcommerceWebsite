using System;
using System.Collections.Generic;
using Ecommerce.Core.Models.Enums;

namespace Ecommerce.Core.Models.Entities
{
    public class Product : BaseEntity
    {
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductSlug { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductDiscountPrice { get; set; }
        public int ProductStock { get; set; }
        public string ProductSku { get; set; }
        public ProductStatus ProductStatus { get; set; } = ProductStatus.Active;
        public bool IsFeatured { get; set; } = false;
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        
        public Guid CategoryId { get; set; }
        
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductRating> ProductRatings { get; set; } = new List<ProductRating>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
} 
