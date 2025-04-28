using Ecommerce.Core.Models.Enums;
using System;
using System.Collections.Generic;

namespace Ecommerce.Core.DTOs
{
    public class OrderListDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string Note { get; set; }
    }

    public class OrderListResponseDto
    {
        public IEnumerable<OrderListDto> Orders { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class OrderFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public OrderStatus? Status { get; set; }
        public string SearchTerm { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool IsDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 