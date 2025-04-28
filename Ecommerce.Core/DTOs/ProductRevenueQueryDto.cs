using System;

namespace Ecommerce.Core.DTOs
{
    public class ProductRevenueQueryDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }
    }
} 