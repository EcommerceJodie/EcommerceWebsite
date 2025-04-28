using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Exceptions;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.Implementations
{
    public class ProductRevenueService : IProductRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public ProductRevenueService(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = _unitOfWork.Repository<Order>();
            _orderDetailRepository = _unitOfWork.Repository<OrderDetail>();
            _productRepository = _unitOfWork.Repository<Product>();
            _categoryRepository = _unitOfWork.Repository<Category>();
            _mapper = mapper;
        }

        public async Task<ProductRevenueResponseDto> GetProductRevenueAsync(ProductRevenueQueryDto queryDto)
        {
            // Chuẩn bị khoảng thời gian truy vấn
            var fromDate = queryDto.FromDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
            var toDate = queryDto.ToDate?.Date.AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1);

            // Lấy thông tin về các đơn hàng đã hoàn thành (đã giao)
            var completedOrders = await _orderRepository.Ts
                .Where(o => o.OrderStatus == OrderStatus.Processing)
                .Where(o => o.CreatedAt >= fromDate && o.CreatedAt < toDate)
                .ToListAsync();

            if (!completedOrders.Any())
            {
                return new ProductRevenueResponseDto
                {
                    Products = new List<ProductRevenueDto>(),
                    GrandTotalRevenue = 0,
                    GrandTotalOrderCount = 0,
                    GrandTotalQuantitySold = 0
                };
            }

            // Lấy chi tiết đơn hàng
            var orderIds = completedOrders.Select(o => o.Id).ToList();
            var orderDetails = await _orderDetailRepository.Ts
                .Where(od => orderIds.Contains(od.OrderId))
                .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                .ToListAsync();

            // Nếu có lọc theo sản phẩm cụ thể
            if (queryDto.ProductId.HasValue)
            {
                orderDetails = orderDetails.Where(od => od.ProductId == queryDto.ProductId.Value).ToList();
            }

            // Nếu có lọc theo danh mục
            if (queryDto.CategoryId.HasValue)
            {
                orderDetails = orderDetails.Where(od => od.Product.CategoryId == queryDto.CategoryId.Value).ToList();
            }

            // Nhóm dữ liệu theo sản phẩm
            var productGroups = orderDetails
                .GroupBy(od => new 
                { 
                    od.ProductId, 
                    ProductName = od.Product.ProductName,
                    ProductSku = od.Product.ProductSku,
                    CategoryName = od.Product.Category?.CategoryName ?? "Không có danh mục"
                })
                .Select(g => new ProductRevenueDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    ProductSku = g.Key.ProductSku,
                    CategoryName = g.Key.CategoryName,
                    TotalRevenue = g.Sum(od => od.Subtotal),
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalOrderCount = g.Select(od => od.OrderId).Distinct().Count(),
                    DailyRevenue = g.GroupBy(od => od.Order.CreatedAt.Date)
                        .Select(dg => new DailyRevenueDto
                        {
                            Date = dg.Key,
                            TotalRevenue = dg.Sum(od => od.Subtotal),
                            QuantitySold = dg.Sum(od => od.Quantity),
                            OrderCount = dg.Select(od => od.OrderId).Distinct().Count()
                        })
                        .Where(dr => dr.TotalRevenue > 0)
                        .OrderBy(dr => dr.Date)
                        .ToList()
                })
                .ToList();

            // Tính tổng doanh thu
            var response = new ProductRevenueResponseDto
            {
                Products = productGroups.OrderByDescending(p => p.TotalRevenue).ToList(),
                GrandTotalRevenue = productGroups.Sum(p => p.TotalRevenue),
                GrandTotalQuantitySold = productGroups.Sum(p => p.TotalQuantitySold),
                GrandTotalOrderCount = completedOrders.Count
            };

            return response;
        }

        public async Task<ProductRevenueDto> GetProductRevenueDetailAsync(Guid productId, DateTime? fromDate, DateTime? toDate)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.IsDeleted)
            {
                throw new EntityNotFoundException("Sản phẩm", productId);
            }

            // Tạo query với sản phẩm đã chọn
            var query = new ProductRevenueQueryDto
            {
                ProductId = productId,
                FromDate = fromDate,
                ToDate = toDate
            };

            // Lấy thông tin doanh thu
            var result = await GetProductRevenueAsync(query);
            
            // Nếu không có dữ liệu, tạo một bản ghi trống
            if (!result.Products.Any())
            {
                var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
                return new ProductRevenueDto
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    ProductSku = product.ProductSku,
                    CategoryName = category?.CategoryName ?? "Không có danh mục",
                    TotalRevenue = 0,
                    TotalQuantitySold = 0,
                    TotalOrderCount = 0,
                    DailyRevenue = new List<DailyRevenueDto>()
                };
            }

            return result.Products.First();
        }
    }
} 