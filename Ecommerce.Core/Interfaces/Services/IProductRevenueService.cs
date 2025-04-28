using System;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface IProductRevenueService
    {
        /// <summary>
        /// Lấy doanh thu theo ngày của sản phẩm
        /// </summary>
        /// <param name="queryDto">Tham số truy vấn doanh thu</param>
        /// <returns>Thông tin doanh thu của sản phẩm theo ngày</returns>
        Task<ProductRevenueResponseDto> GetProductRevenueAsync(ProductRevenueQueryDto queryDto);
        
        /// <summary>
        /// Lấy doanh thu chi tiết của một sản phẩm cụ thể
        /// </summary>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="fromDate">Ngày bắt đầu</param>
        /// <param name="toDate">Ngày kết thúc</param>
        /// <returns>Thông tin doanh thu chi tiết của sản phẩm</returns>
        Task<ProductRevenueDto> GetProductRevenueDetailAsync(Guid productId, DateTime? fromDate, DateTime? toDate);
    }
} 