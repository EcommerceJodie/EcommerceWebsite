using System;

namespace Ecommerce.Core.DTOs
{
    public class ProductQueryDto : PaginationRequestDto
    {
        public Guid? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? InStock { get; set; }
        public string Status { get; set; }
    }
} 
