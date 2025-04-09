using System;
using System.Collections.Generic;

namespace Ecommerce.Core.DTOs
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? LastActive { get; set; }
        public string CartStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
} 