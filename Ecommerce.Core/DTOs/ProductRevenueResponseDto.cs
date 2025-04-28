using System.Collections.Generic;

namespace Ecommerce.Core.DTOs
{
    public class ProductRevenueResponseDto
    {
        public List<ProductRevenueDto> Products { get; set; } = new List<ProductRevenueDto>();
        public decimal GrandTotalRevenue { get; set; }
        public int GrandTotalQuantitySold { get; set; }
        public int GrandTotalOrderCount { get; set; }
    }
} 