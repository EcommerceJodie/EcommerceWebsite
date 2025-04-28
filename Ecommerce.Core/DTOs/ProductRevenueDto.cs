using System;
using System.Collections.Generic;

namespace Ecommerce.Core.DTOs
{
    public class ProductRevenueDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public string CategoryName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalQuantitySold { get; set; }
        public int TotalOrderCount { get; set; }
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new List<DailyRevenueDto>();
    }
} 