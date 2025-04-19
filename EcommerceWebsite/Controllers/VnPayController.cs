using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Core.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Ecommerce.Shared.Utils;
using Microsoft.Extensions.Logging;

namespace EcommerceWebsite.Controllers
{
    public class VnPayController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayController> _logger;

        public VnPayController(
            IOrderService orderService,
            IPaymentTransactionService paymentTransactionService,
            IConfiguration configuration,
            ILogger<VnPayController> logger)
        {
            _orderService = orderService;
            _paymentTransactionService = paymentTransactionService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        [Route("vnpay-return")]
        public async Task<IActionResult> PaymentReturn()
        {
            _logger.LogInformation("Nhận kết quả thanh toán từ VNPay - Return URL");
            try
            {
                var transaction = await _paymentTransactionService.ProcessVnPayResponseAsync(Request.Query);
                
                if (transaction.Status == PaymentStatus.Completed)
                {
                    _logger.LogInformation($"Thanh toán thành công cho đơn hàng {transaction.OrderNumber}");
                    return RedirectToAction("OrderSuccess", "Checkout", new { id = transaction.OrderId });
                }
                else
                {
                    _logger.LogWarning($"Thanh toán thất bại cho đơn hàng {transaction.OrderNumber}. Mã lỗi: {transaction.ResponseCode}");
                    TempData["ErrorMessage"] = $"Thanh toán thất bại. {transaction.Notes}";
                    return RedirectToAction("OrderFailed", "Checkout");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý kết quả thanh toán từ VNPay");
                TempData["ErrorMessage"] = $"Lỗi xử lý thanh toán: {ex.Message}";
                return RedirectToAction("OrderFailed", "Checkout");
            }
        }

        [HttpGet]
        [Route("vnpay-ipn")]
        public async Task<IActionResult> PaymentIPN()
        {
            _logger.LogInformation("Nhận thông báo IPN từ VNPay - IPN URL");
            try
            {
                var transaction = await _paymentTransactionService.ProcessVnPayResponseAsync(Request.Query);
                _logger.LogInformation($"Đã xử lý IPN cho đơn hàng {transaction.OrderNumber}, trạng thái: {transaction.Status}");
                

                return Ok(new 
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý IPN từ VNPay");
                return Ok(new 
                {
                    RspCode = "99",
                    Message = $"Lỗi xử lý: {ex.Message}"
                });
            }
        }
    }
} 
