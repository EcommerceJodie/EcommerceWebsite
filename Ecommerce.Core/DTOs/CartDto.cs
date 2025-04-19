using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecommerce.Core.DTOs
{
    public class CartDto
    {
        public string CartId { get; set; }
        public string CustomerId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int TotalItems => Items.Sum(item => item.Quantity);
        public decimal TotalAmount => Items.Sum(item => item.Subtotal);
    }
} 
