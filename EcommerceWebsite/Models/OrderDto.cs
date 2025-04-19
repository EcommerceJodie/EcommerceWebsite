using System;
using System.Collections.Generic;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Core.DTOs;

namespace EcommerceWebsite.Models
{

    public class OrderDto
    {
        private readonly Ecommerce.Core.DTOs.OrderDto _coreOrderDto;

        public OrderDto(Ecommerce.Core.DTOs.OrderDto coreOrderDto)
        {
            _coreOrderDto = coreOrderDto;
        }

        public Guid Id => _coreOrderDto.Id;
        public string OrderNumber => _coreOrderDto.OrderNumber;
        public decimal TotalAmount => _coreOrderDto.TotalAmount;
        public OrderStatus OrderStatus => _coreOrderDto.OrderStatus;
        public string ShippingAddress => _coreOrderDto.ShippingAddress;
        public string ShippingCity => _coreOrderDto.ShippingCity;
        public string ShippingPostalCode => _coreOrderDto.ShippingPostalCode;
        public string ShippingCountry => _coreOrderDto.ShippingCountry;
        public string PaymentMethod => _coreOrderDto.PaymentMethod;
        public string PaymentTransactionId => _coreOrderDto.PaymentTransactionId;
        public DateTime? PaymentDate => _coreOrderDto.PaymentDate;
        public DateTime? ShippingDate => _coreOrderDto.ShippingDate;
        public string Notes => _coreOrderDto.Notes;
        public Guid CustomerId => _coreOrderDto.CustomerId;
        public string CustomerName => _coreOrderDto.CustomerName;
        public DateTime CreatedAt => _coreOrderDto.CreatedAt;
        public List<Ecommerce.Core.DTOs.OrderDetailDto> OrderDetails => _coreOrderDto.OrderDetails;


        public static implicit operator OrderDto(Ecommerce.Core.DTOs.OrderDto dto) => new OrderDto(dto);
    }
} 
