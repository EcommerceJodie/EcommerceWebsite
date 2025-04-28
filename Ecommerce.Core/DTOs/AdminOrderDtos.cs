using Ecommerce.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.DTOs
{
    public class AdminCreateOrderDto
    {
        // Thông tin khách hàng
        [Required(ErrorMessage = "ID khách hàng là bắt buộc")]
        public Guid CustomerId { get; set; }
        
        [Required(ErrorMessage = "Số điện thoại khách hàng là bắt buộc")]
        public string PhoneNumber { get; set; }
        
        // Thông tin giao hàng
        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 200 ký tự")]
        public string ShippingAddress { get; set; }
        
        // Ghi chú
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string Note { get; set; }
        
        // Các sản phẩm trong đơn hàng
        [Required(ErrorMessage = "Đơn hàng phải có ít nhất một sản phẩm")]
        public List<SimpleOrderItemDto> OrderItems { get; set; } = new List<SimpleOrderItemDto>();
    }

    public class SimpleOrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }

    public class OrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal Discount { get; set; } = 0;
    }

    public class CustomerSearchResultDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public string City { get; set; }
        public string Country { get; set; }
    }

    public class OrderBillDto
    {
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string PaymentMethod { get; set; }
        
        public decimal Subtotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TotalAmount { get; set; }
        
        public List<OrderDetailDto> Items { get; set; } = new List<OrderDetailDto>();
        public string Notes { get; set; }
        
        // Thông tin người tạo đơn
        public string CreatedBy { get; set; }
    }
} 