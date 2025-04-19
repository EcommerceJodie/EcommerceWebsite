using System;
using System.Collections.Generic;
using Ecommerce.Core.Models.Enums;

namespace Ecommerce.Core.Models.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }
        public string PaymentMethod { get; set; }
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string Notes { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    }
} 
