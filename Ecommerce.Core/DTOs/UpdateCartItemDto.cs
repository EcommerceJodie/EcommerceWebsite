using System;

namespace Ecommerce.Core.DTOs
{
    public class UpdateCartItemDto
    {
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
} 
