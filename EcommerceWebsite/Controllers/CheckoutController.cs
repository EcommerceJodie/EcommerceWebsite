using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using EcommerceWebsite.Models;
using Microsoft.Extensions.Logging;
using Ecommerce.Core.Models.Enums;
using Microsoft.Extensions.Configuration;

namespace EcommerceWebsite.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPaymentTransactionService _paymentTransactionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICartService cartService, 
            IProductService productService,
            IOrderService orderService,
            IPaymentTransactionService paymentTransactionService,
            IConfiguration configuration,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _orderService = orderService;
            _paymentTransactionService = paymentTransactionService;
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var cartId = GetCartId();
            if (string.IsNullOrEmpty(cartId))
            {
                return RedirectToAction("Index", "Cart");
            }

            var cart = _cartService.GetCartAsync(cartId).Result;
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<string> productIds)
        {
            _logger.LogInformation($"Đã nhận Index POST với {productIds?.Count ?? 0} sản phẩm");
            
            if (productIds == null || !productIds.Any())
            {
                _logger.LogWarning("Không có sản phẩm nào được chọn");
                return RedirectToAction("Index", "Cart");
            }

            var cartId = GetCartId();
            if (string.IsNullOrEmpty(cartId))
            {
                _logger.LogWarning("Không tìm thấy cartId");
                return RedirectToAction("Index", "Cart");
            }

            var cart = await _cartService.GetCartAsync(cartId);
            if (cart == null || cart.Items.Count == 0)
            {
                _logger.LogWarning("Giỏ hàng trống");
                return RedirectToAction("Index", "Cart");
            }


            var selectedItems = cart.Items.Where(item => productIds.Contains(item.ProductId.ToString())).ToList();
            if (!selectedItems.Any())
            {
                _logger.LogWarning("Không tìm thấy sản phẩm nào khớp với productIds");
                return RedirectToAction("Index", "Cart");
            }


            var selectedCart = new CartDto
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                Items = selectedItems
            };


            var productIdsJson = System.Text.Json.JsonSerializer.Serialize(productIds);
            _logger.LogInformation($"Lưu vào TempData: {productIdsJson}");
            TempData["SelectedProductIds"] = productIdsJson;

            return View(selectedCart);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(CreateOrderDto createOrderDto)
        {
            _logger.LogInformation("=== BẮT ĐẦU XỬ LÝ PLACE ORDER ===");
            _logger.LogInformation($"Request nhận được: {System.Text.Json.JsonSerializer.Serialize(createOrderDto)}");
            

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ: " + string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)));
                

                if (ModelState["ProductIds"] != null)
                {
                    ModelState.Remove("ProductIds");
                }
                

                if (!ModelState.IsValid)
                {
                    return RedirectToAction("Index");
                }
            }

            try
            {

                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation($"Customer ID từ claim: {customerId}");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("Không tìm thấy customerId trong claim");
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
                }
                

                createOrderDto.CustomerId = Guid.Parse(customerId);
                _logger.LogInformation($"Đã set CustomerId: {createOrderDto.CustomerId}");


                _logger.LogInformation("=== KIỂM TRA TEMPDATA ===");
                _logger.LogInformation($"Có TempData? {TempData != null}");
                _logger.LogInformation($"TempData Count: {TempData?.Count() ?? 0}");
                foreach (var key in TempData?.Keys ?? Array.Empty<string>())
                {
                    _logger.LogInformation($"TempData Key: {key}, Value: {TempData[key]}");
                }
                

                if (Request.Form.ContainsKey("productIds"))
                {
                    var formProductIds = Request.Form["productIds"].ToList();
                    _logger.LogInformation($"Form productIds count: {formProductIds.Count}");
                    if (formProductIds.Any())
                    {
                        var productIdsJson = System.Text.Json.JsonSerializer.Serialize(formProductIds);
                        TempData["SelectedProductIds"] = productIdsJson;
                        _logger.LogInformation($"Đã lưu productIds từ form vào TempData: {productIdsJson}");
                    }
                }
                
                if (TempData["SelectedProductIds"] != null)
                {
                    _logger.LogInformation($"Loại của TempData: {TempData["SelectedProductIds"].GetType().FullName}");
                    _logger.LogInformation($"Giá trị của TempData: {TempData["SelectedProductIds"]}");
                    

                    if (TempData["SelectedProductIds"] is string tempDataString)
                    {
                        _logger.LogInformation("TempData là chuỗi, đang chuyển đổi...");
                        var selectedProductIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(tempDataString);
                        if (selectedProductIds != null && selectedProductIds.Any())
                        {
                            createOrderDto.ProductIds = selectedProductIds.Select(id => Guid.Parse(id)).ToList();
                            _logger.LogInformation($"Đã lấy được {createOrderDto.ProductIds.Count} sản phẩm từ TempData");
                        }
                    }
                    else if (TempData["SelectedProductIds"] is List<string> selectedProductIds && selectedProductIds.Any())
                    {
                        _logger.LogInformation($"TempData là List<string>, số lượng: {selectedProductIds.Count}");
                        createOrderDto.ProductIds = selectedProductIds.Select(id => Guid.Parse(id)).ToList();
                    }
                    else if (TempData["SelectedProductIds"] is IEnumerable<object> objects)
                    {
                        _logger.LogInformation($"TempData là IEnumerable<object>, số lượng: {objects.Count()}");
                        createOrderDto.ProductIds = objects.Select(o => Guid.Parse(o.ToString())).ToList();
                    }
                }
                else
                {
                    _logger.LogWarning("Không tìm thấy SelectedProductIds trong TempData");
                    

                    TempData["ErrorMessage"] = "Không tìm thấy thông tin sản phẩm được chọn. Vui lòng thử lại.";
                    return RedirectToAction("Index", "Cart");
                }

                if (createOrderDto.ProductIds == null || !createOrderDto.ProductIds.Any())
                {
                    _logger.LogWarning("Không có sản phẩm nào được chọn để thanh toán");
                    TempData["ErrorMessage"] = "Không có sản phẩm nào được chọn để thanh toán";
                    return RedirectToAction("Index", "Cart");
                }
                

                _logger.LogInformation($"Thông tin đơn hàng: " +
                    $"CustomerId={createOrderDto.CustomerId}, " +
                    $"ShippingAddress={createOrderDto.ShippingAddress}, " +
                    $"City={createOrderDto.ShippingCity}, " +
                    $"ProductIds={string.Join(",", createOrderDto.ProductIds)}");


                _logger.LogInformation("Bắt đầu tạo đơn hàng...");
                var order = await _orderService.CreateOrderAsync(createOrderDto);
                _logger.LogInformation($"Đã tạo đơn hàng thành công, OrderId: {order.Id}");


                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                var returnUrl = $"{baseUrl}/vnpay-return";
                _logger.LogInformation($"Return URL cho VNPay: {returnUrl}");
                
                _logger.LogInformation("Bắt đầu tạo URL thanh toán VNPay...");
                var paymentUrl = await _orderService.CreatePaymentUrlAsync(order.Id, returnUrl);
                _logger.LogInformation($"Đã tạo URL thanh toán: {paymentUrl}");


                var debugShowUrl = _configuration.GetValue<bool>("AppSettings:Debug:ShowPaymentUrl", false);
                var debugSkipPayment = _configuration.GetValue<bool>("AppSettings:Debug:SkipPaymentGateway", false);
                
                _logger.LogInformation($"Debug Settings - ShowPaymentUrl: {debugShowUrl}, SkipPayment: {debugSkipPayment}");


                if (debugShowUrl)
                {
                    TempData["DebugInfo"] = paymentUrl;
                }


                if (debugSkipPayment)
                {
                    ViewBag.PaymentUrl = paymentUrl;
                    ViewBag.OrderId = order.Id;
                    ViewBag.TotalAmount = order.TotalAmount;
                    return View("DebugPayment");
                }


                _logger.LogInformation("=== KẾT THÚC XỬ LÝ - CHUYỂN HƯỚNG ĐẾN VNPAY ===");
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "=== LỖI KHI XỬ LÝ ĐƠN HÀNG ===");
                _logger.LogError($"Chi tiết ngoại lệ: {ex.ToString()}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.ToString()}");
                }
                TempData["ErrorMessage"] = $"Lỗi khi đặt hàng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> PaymentReturn()
        {
            try
            {

                var transaction = await _paymentTransactionService.ProcessVnPayResponseAsync(Request.Query);
                

                var order = await _orderService.GetOrderByIdAsync(transaction.OrderId);


                if (transaction.Status == PaymentStatus.Completed)

                {

                    TempData["SuccessMessage"] = "Thanh toán thành công. Cảm ơn bạn đã đặt hàng!";
                    return RedirectToAction("OrderSuccess", new { id = order.Id });
                }
                else
                {

                    TempData["ErrorMessage"] = $"Thanh toán thất bại. {transaction.Notes}";
                    return RedirectToAction("OrderFailed");
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Lỗi xử lý kết quả thanh toán từ VNPAY");
                TempData["ErrorMessage"] = $"Lỗi xử lý thanh toán: {ex.Message}";
                return RedirectToAction("OrderFailed");
            }
        }

        [Authorize]
        public async Task<IActionResult> OrderSuccess(Guid id)
        {
            var orderCore = await _orderService.GetOrderByIdAsync(id);
            if (orderCore == null)
            {
                return NotFound();
            }


            var order = new EcommerceWebsite.Models.OrderDto(orderCore);
            return View(orderCore); 
        }

        [Authorize]
        public IActionResult OrderFailed()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult DebugPayment()
        {

            if (TempData["DebugInfo"] != null)
            {
                ViewBag.PaymentUrl = TempData["DebugInfo"].ToString();
            }
            return View();
        }

        private string GetCartId()
        {
            if (User.Identity.IsAuthenticated)
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else if (Request.Cookies.TryGetValue("cart_id", out string cartId) && !string.IsNullOrEmpty(cartId))
            {
                return cartId;
            }
            return null;
        }
    }
} 

