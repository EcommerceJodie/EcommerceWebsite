using System;

namespace Ecommerce.Core.Models.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; }
        public string ImageAltText { get; set; }
        public bool IsMainImage { get; set; }
        public int DisplayOrder { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
} 