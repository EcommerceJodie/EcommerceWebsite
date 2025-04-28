using System;

namespace Ecommerce.Core.DTOs
{
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public int QuantitySold { get; set; }
    }
} 