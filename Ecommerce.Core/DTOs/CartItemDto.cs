using System;

namespace Ecommerce.Core.DTOs
{
    public class CartItemDto
    {
        public string CartId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSlug { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal Subtotal => DiscountPrice.HasValue && DiscountPrice.Value > 0 
            ? DiscountPrice.Value * Quantity 
            : UnitPrice * Quantity;
    }
} 
