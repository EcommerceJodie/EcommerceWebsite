using System;

namespace Ecommerce.Core.Models.Entities
{
    public class ProductRating : BaseEntity
    {
        public int Rating { get; set; }
        public string ReviewTitle { get; set; }
        public string ReviewText { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; } = false;
        public Guid ProductId { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Product Product { get; set; }
        public virtual Customer Customer { get; set; }
    }
} 