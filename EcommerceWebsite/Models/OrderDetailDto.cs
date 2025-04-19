using System;

namespace EcommerceWebsite.Models
{

    public class OrderDetailDto
    {
        private readonly Ecommerce.Core.DTOs.OrderDetailDto _coreOrderDetailDto;

        public OrderDetailDto(Ecommerce.Core.DTOs.OrderDetailDto coreOrderDetailDto)
        {
            _coreOrderDetailDto = coreOrderDetailDto;
        }

        public Guid Id => _coreOrderDetailDto.Id;
        public int Quantity => _coreOrderDetailDto.Quantity;
        public decimal UnitPrice => _coreOrderDetailDto.UnitPrice;
        public decimal Subtotal => _coreOrderDetailDto.Subtotal;
        public decimal Discount => _coreOrderDetailDto.Discount;
        public Guid ProductId => _coreOrderDetailDto.ProductId;
        public string ProductName => _coreOrderDetailDto.ProductName;
        public string ProductImageUrl => _coreOrderDetailDto.ProductImageUrl;


        public static implicit operator OrderDetailDto(Ecommerce.Core.DTOs.OrderDetailDto dto) => new OrderDetailDto(dto);
    }
} 
