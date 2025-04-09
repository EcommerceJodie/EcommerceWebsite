using System;

namespace Ecommerce.Core.Models.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; } = 0;
        
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
} 