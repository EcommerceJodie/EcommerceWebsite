using Ecommerce.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string Notes { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
    }

    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
    }

    public class CreateOrderDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 200 ký tự")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Thành phố là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên thành phố không được vượt quá 100 ký tự")]
        public string ShippingCity { get; set; }

        [StringLength(20, ErrorMessage = "Mã bưu điện không được vượt quá 20 ký tự")]
        public string ShippingPostalCode { get; set; }

        [Required(ErrorMessage = "Quốc gia là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên quốc gia không được vượt quá 100 ký tự")]
        public string ShippingCountry { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string Notes { get; set; }



        public List<Guid> ProductIds { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
        public string PaymentMethod { get; set; } = "VNPAY";
    }
} 
