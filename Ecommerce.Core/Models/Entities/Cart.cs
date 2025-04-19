using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Models.Entities
{
    public class Cart : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public DateTime? LastActive { get; set; }
        public string CartStatus { get; set; } = "Active"; 
        public virtual Customer Customer { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
} 
