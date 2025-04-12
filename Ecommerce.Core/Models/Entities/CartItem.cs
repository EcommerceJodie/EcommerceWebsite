using System;

namespace Ecommerce.Core.Models.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
        //luu vao redis...
    }
} 