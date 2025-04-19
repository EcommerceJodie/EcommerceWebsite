using Ecommerce.Core.DTOs;
using Ecommerce.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId);
        Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status);
        Task<string> CreatePaymentUrlAsync(Guid orderId, string returnUrl);
        Task<OrderDto> ProcessPaymentReturnAsync(IQueryCollection queryCollection);
    }
} 
