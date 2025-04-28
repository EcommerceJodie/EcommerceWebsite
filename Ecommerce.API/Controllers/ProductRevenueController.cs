using System;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductRevenueController : ControllerBase
    {
        private readonly IProductRevenueService _productRevenueService;

        public ProductRevenueController(IProductRevenueService productRevenueService)
        {
            _productRevenueService = productRevenueService;
        }

        /// <summary>
        /// Lấy doanh thu của sản phẩm theo khoảng thời gian (Query Parameters)
        /// </summary>
        /// <param name="queryDto">Tham số truy vấn</param>
        /// <returns>Thông tin doanh thu của sản phẩm</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProductRevenueResponseDto), 200)]
        public async Task<IActionResult> GetProductRevenue([FromQuery] ProductRevenueQueryDto queryDto)
        {
            var result = await _productRevenueService.GetProductRevenueAsync(queryDto);
            return Ok(result);
        }

        /// <summary>
        /// Lấy doanh thu của sản phẩm theo khoảng thời gian (JSON Body)
        /// </summary>
        /// <param name="queryDto">Tham số truy vấn trong JSON body</param>
        /// <returns>Thông tin doanh thu của sản phẩm</returns>
        [HttpPost("filter")]
        [ProducesResponseType(typeof(ProductRevenueResponseDto), 200)]
        public async Task<IActionResult> FilterProductRevenue([FromBody] ProductRevenueQueryDto queryDto)
        {
            var result = await _productRevenueService.GetProductRevenueAsync(queryDto);
            return Ok(result);
        }

        /// <summary>
        /// Lấy doanh thu của sản phẩm theo khoảng thời gian (Form Data)
        /// </summary>
        /// <returns>Thông tin doanh thu của sản phẩm</returns>
        [HttpPost("filter-form")]
        [ProducesResponseType(typeof(ProductRevenueResponseDto), 200)]
        public async Task<IActionResult> FilterProductRevenueForm([FromForm] ProductRevenueQueryDto queryDto)
        {
            var result = await _productRevenueService.GetProductRevenueAsync(queryDto);
            return Ok(result);
        }

        /// <summary>
        /// Lấy doanh thu chi tiết của một sản phẩm cụ thể
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <param name="fromDate">Ngày bắt đầu (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Ngày kết thúc (định dạng yyyy-MM-dd)</param>
        /// <returns>Thông tin doanh thu chi tiết của sản phẩm</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductRevenueDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductRevenueDetail(
            Guid id, 
            [FromQuery] DateTime? fromDate, 
            [FromQuery] DateTime? toDate)
        {
            var result = await _productRevenueService.GetProductRevenueDetailAsync(id, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Lấy doanh thu chi tiết của một sản phẩm cụ thể với tham số trong body
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <param name="queryDto">Tham số truy vấn trong JSON body</param>
        /// <returns>Thông tin doanh thu chi tiết của sản phẩm</returns>
        [HttpPost("{id}/filter")]
        [ProducesResponseType(typeof(ProductRevenueDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> FilterProductRevenueDetail(
            Guid id,
            [FromBody] ProductRevenueQueryDto queryDto)
        {
            // Đảm bảo sử dụng ID từ đường dẫn, bỏ qua ProductId trong queryDto
            var result = await _productRevenueService.GetProductRevenueDetailAsync(id, queryDto.FromDate, queryDto.ToDate);
            return Ok(result);
        }
    }
} 