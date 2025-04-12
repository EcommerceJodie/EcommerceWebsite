using System;

namespace Ecommerce.Core.DTOs
{
    public class AddToCartDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
} 