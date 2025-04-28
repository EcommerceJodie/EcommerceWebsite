using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Core.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Ecommerce.API.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminOrdersController> _logger;

        public AdminOrdersController(
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminOrdersController> logger)
        {
            _orderService = orderService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách đơn hàng có phân trang
        /// </summary>
        /// <remarks>
        /// Mẫu yêu cầu:
        /// 
        /// GET /api/admin/orders?pageNumber=1&pageSize=10&fromDate=2023-01-01&toDate=2023-12-31&status=Pending&searchTerm=nguyenvan&sortBy=CreatedAt&isDescending=true
        /// 
        /// Tất cả các tham số đều là tùy chọn. Nếu không cung cấp, sẽ sử dụng giá trị mặc định:
        /// - pageNumber: 1
        /// - pageSize: 10
        /// - sortBy: "CreatedAt"
        /// - isDescending: true
        /// </remarks>
        /// <param name="fromDate">Ngày bắt đầu (yyyy-MM-dd)</param>
        /// <param name="toDate">Ngày kết thúc (yyyy-MM-dd)</param>
        /// <param name="status">Trạng thái đơn hàng (Pending, Processing, Shipped, Delivered, Cancelled)</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên khách hàng, số điện thoại, mã đơn hàng)</param>
        /// <param name="sortBy">Sắp xếp theo trường (CreatedAt, OrderNumber, CustomerName, TotalAmount)</param>
        /// <param name="isDescending">Sắp xếp giảm dần (true) hoặc tăng dần (false)</param>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng đơn hàng trên một trang</param>
        /// <returns>Danh sách đơn hàng và thông tin phân trang</returns>
        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] Ecommerce.Core.Models.Enums.OrderStatus? status,
            [FromQuery] string searchTerm = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool isDescending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filterDto = new OrderFilterDto
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Status = status,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    IsDescending = isDescending,
                    PageNumber = pageNumber < 1 ? 1 : pageNumber,
                    PageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize)
                };

                var result = await _orderService.GetOrdersAsync(filterDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng: {Message}", ex.Message);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách đơn hàng", details = ex.Message });
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới từ trang quản trị
        /// </summary>
        /// <remarks>
        /// Mẫu yêu cầu:
        /// 
        /// POST /api/admin/orders
        /// {
        ///   "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "phoneNumber": "0901234567",
        ///   "shippingAddress": "123 Nguyễn Huệ, Quận 1, TP.HCM",
        ///   "note": "Giao hàng trong giờ hành chính",
        ///   "orderItems": [
        ///     {
        ///       "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "quantity": 2
        ///     },
        ///     {
        ///       "productId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        ///       "quantity": 1
        ///     }
        ///   ]
        /// }
        /// </remarks>
        /// <param name="createOrderDto">Thông tin đơn hàng</param>
        /// <returns>Thông tin đơn hàng vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] AdminCreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _orderService.CreateOrderFromAdminAsync(createOrderDto, userId);
                return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin đơn hàng theo ID
        /// </summary>
        /// <param name="id">ID đơn hàng</param>
        /// <returns>Thông tin đơn hàng</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = $"Không tìm thấy đơn hàng với ID: {id}" });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin đơn hàng: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy hoá đơn của đơn hàng để in
        /// </summary>
        /// <param name="id">ID đơn hàng</param>
        /// <returns>Thông tin hoá đơn</returns>
        [HttpGet("{id}/bill")]
        public async Task<IActionResult> GetOrderBill(Guid id)
        {
            try
            {
                var bill = await _orderService.GetOrderBillAsync(id);
                return Ok(bill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin hóa đơn: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại cần tìm</param>
        /// <returns>Danh sách khách hàng phù hợp</returns>
        [HttpGet("search-customers")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return BadRequest(new { message = "Vui lòng nhập số điện thoại để tìm kiếm" });
            }

            try
            {
                var customers = await _orderService.SearchCustomersByPhoneAsync(phoneNumber);
                return Ok(customers);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Lỗi đầu vào khi tìm kiếm khách hàng: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm khách hàng: {Message}", ex.Message);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm kiếm khách hàng", details = ex.Message });
            }
        }
    }
} 