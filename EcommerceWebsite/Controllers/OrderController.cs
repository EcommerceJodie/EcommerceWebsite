using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcommerceWebsite.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _logger = logger;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> History()
        {
            try
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Không tìm thấy userId trong claim");
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("History", "Order") });
                }

                _logger.LogInformation($"Đang tìm đơn hàng cho userId: {userId}");
                

                var customer = await _unitOfWork.Repository<Customer>().Ts
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogWarning($"Không tìm thấy customer cho userID: {userId}");

                    TempData["ErrorMessage"] = "Chưa có thông tin khách hàng hoặc bạn chưa đặt đơn hàng nào.";
                    return View(Enumerable.Empty<OrderDto>());
                }
                
                _logger.LogInformation($"Tìm thấy customer với ID: {customer.Id}, Email: {customer.Email}");
                

                var orderRepository = _unitOfWork.Repository<Order>();
                var orders = await orderRepository.Ts
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.ProductImages)
                    .Include(o => o.Customer)
                    .Where(o => o.Customer.UserId == userId || o.CustomerId == Guid.Parse(userId))
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation($"Tìm thấy {orders.Count} đơn hàng cho user: {userId}");
                

                var orderDtos = orders.Select(order => new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    OrderStatus = order.OrderStatus,
                    ShippingAddress = order.ShippingAddress,
                    ShippingCity = order.ShippingCity,
                    ShippingPostalCode = order.ShippingPostalCode,
                    ShippingCountry = order.ShippingCountry,
                    PaymentMethod = order.PaymentMethod,
                    PaymentTransactionId = order.PaymentTransactionId,
                    PaymentDate = order.PaymentDate,
                    ShippingDate = order.ShippingDate,
                    Notes = order.Notes,
                    CustomerId = order.CustomerId,
                    CustomerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}",
                    CreatedAt = order.CreatedAt,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                    {
                        Id = od.Id,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        Subtotal = od.Subtotal,
                        Discount = od.Discount,
                        ProductId = od.ProductId,
                        ProductName = od.Product?.ProductName,
                        ProductImageUrl = od.Product?.ProductImages?.FirstOrDefault()?.ImageUrl
                    }).ToList()
                }).ToList();
                
                return View(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử đơn hàng");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải lịch sử đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("History");
                }


                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _unitOfWork.Repository<Customer>().Ts
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    return Forbid();
                }
                
                if (order.CustomerId != customer.Id)
                {
                    _logger.LogWarning($"Người dùng {userId} đang cố xem đơn hàng {id} không thuộc về họ.");
                    return Forbid();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết đơn hàng ID: {id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải chi tiết đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("History");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("History");
                }


                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await _unitOfWork.Repository<Customer>().Ts
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null || order.CustomerId != customer.Id)
                {
                    return Forbid();
                }


                if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái này.";
                    return RedirectToAction("Details", new { id });
                }

                await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled);
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hủy đơn hàng ID: {id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi hủy đơn hàng. Vui lòng thử lại sau.";
                return RedirectToAction("Details", new { id });
            }
        }
    }
} 
