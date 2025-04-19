using System;

namespace Ecommerce.Core.DTOs
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public string ImageAltText { get; set; }
        public bool IsMainImage { get; set; }
        public int DisplayOrder { get; set; }
        public Guid ProductId { get; set; }
    }
} 
