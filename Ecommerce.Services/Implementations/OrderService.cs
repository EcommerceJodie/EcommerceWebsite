using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Models.Enums;
using Ecommerce.Shared.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(
            IUnitOfWork unitOfWork,
            ICartService cartService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();


                var customerRepository = _unitOfWork.Repository<Customer>();

                var userId = createOrderDto.CustomerId.ToString();


                var customer = await customerRepository.Ts
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {

                    var userRepository = _unitOfWork.Repository<ApplicationUser>();
                    var user = await userRepository.Ts.FirstOrDefaultAsync(u => u.Id == userId);
                    
                    if (user == null)
                    {
                        throw new Exception($"Không tìm thấy thông tin người dùng với ID: {userId}");
                    }
                    

                    customer = new Customer
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        FirstName = user.FirstName ?? "",
                        LastName = user.LastName ?? "",
                        Address = createOrderDto.ShippingAddress,
                        City = createOrderDto.ShippingCity,
                        PostalCode = createOrderDto.ShippingPostalCode,
                        Country = createOrderDto.ShippingCountry,
                        IsActive = true
                    };
                    
                    customerRepository.Add(customer);
                    await _unitOfWork.CompleteAsync();
                }
                else
                {

                    bool needUpdate = false;
                    
                    if (customer.Address != createOrderDto.ShippingAddress)
                    {
                        customer.Address = createOrderDto.ShippingAddress;
                        needUpdate = true;
                    }
                    
                    if (customer.City != createOrderDto.ShippingCity)
                    {
                        customer.City = createOrderDto.ShippingCity;
                        needUpdate = true;
                    }
                    
                    if (customer.PostalCode != createOrderDto.ShippingPostalCode)
                    {
                        customer.PostalCode = createOrderDto.ShippingPostalCode;
                        needUpdate = true;
                    }
                    
                    if (customer.Country != createOrderDto.ShippingCountry)
                    {
                        customer.Country = createOrderDto.ShippingCountry;
                        needUpdate = true;
                    }
                    
                    if (needUpdate)
                    {
                        customerRepository.Update(customer);
                        await _unitOfWork.CompleteAsync();
                    }
                }


                var cart = await _cartService.GetCartAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    throw new Exception("Không tìm thấy giỏ hàng hoặc giỏ hàng trống");
                }


                var selectedItems = cart.Items;
                if (createOrderDto.ProductIds != null && createOrderDto.ProductIds.Any())
                {
                    selectedItems = cart.Items
                        .Where(item => createOrderDto.ProductIds.Contains(item.ProductId))
                        .ToList();
                }

                if (!selectedItems.Any())
                {
                    throw new Exception("Không có sản phẩm nào được chọn để thanh toán");
                }


                var orderNumber = $"ORD{DateTime.Now.ToString("yyyyMMddHHmmss")}{userId.Substring(0, 8)}";
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    TotalAmount = selectedItems.Sum(item => (item.DiscountPrice.HasValue ? item.DiscountPrice.Value : item.UnitPrice) * item.Quantity),
                    OrderStatus = OrderStatus.Pending,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    ShippingCity = createOrderDto.ShippingCity,
                    ShippingPostalCode = createOrderDto.ShippingPostalCode,
                    ShippingCountry = createOrderDto.ShippingCountry,
                    PaymentMethod = createOrderDto.PaymentMethod,
                    PaymentTransactionId = null, 
                    CustomerId = customer.Id, 
                    Notes = createOrderDto.Notes
                };

                _unitOfWork.Repository<Order>().Add(order);
                await _unitOfWork.CompleteAsync();


                var orderDetailRepository = _unitOfWork.Repository<OrderDetail>();
                foreach (var item in selectedItems)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"Không tìm thấy sản phẩm với ID: {item.ProductId}");
                    }

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Discount = item.DiscountPrice.HasValue ? (item.UnitPrice - item.DiscountPrice.Value) : 0,
                        Subtotal = (item.DiscountPrice.HasValue ? item.DiscountPrice.Value : item.UnitPrice) * item.Quantity
                    };

                    orderDetailRepository.Add(orderDetail);
                }

                await _unitOfWork.CompleteAsync();


                if (createOrderDto.PaymentMethod.ToUpper() != "VNPAY")
                {

                    if (createOrderDto.ProductIds != null && createOrderDto.ProductIds.Any())
                    {
                        foreach (var productId in createOrderDto.ProductIds)
                        {
                            await _cartService.RemoveFromCartAsync(userId, productId);
                        }
                    }
                }

                await _unitOfWork.CommitTransactionAsync();


                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Lỗi khi tạo đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid id)
        {
            var orderRepository = _unitOfWork.Repository<Order>();
            var order = await orderRepository.Ts
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return null;
            }

            var orderDto = new OrderDto
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
            };

            return orderDto;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId)
        {
            var orderRepository = _unitOfWork.Repository<Order>();
            var orders = await orderRepository.Ts
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.ProductImages)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

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

            return orderDtos;
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var orderRepository = _unitOfWork.Repository<Order>();
            var order = await orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return null;
            }

            order.OrderStatus = status;
            if (status == OrderStatus.Processing)
            {
                order.PaymentDate = DateTime.Now;
            }
            else if (status == OrderStatus.Shipped)
            {
                order.ShippingDate = DateTime.Now;
            }

            orderRepository.Update(order);
            await _unitOfWork.CompleteAsync();

            return await GetOrderByIdAsync(id);
        }

        public async Task<string> CreatePaymentUrlAsync(Guid orderId, string returnUrl)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng");
            }


            var vnpUrl = _configuration["Payment:VnPay:Url"];
            var vnpTmnCode = _configuration["Payment:VnPay:TmnCode"];
            var vnpHashSecret = _configuration["Payment:VnPay:HashSecret"];
            var vnpReturnUrl = returnUrl;

            Console.WriteLine($"[DEBUG-VNPAY] URL: {vnpUrl}");
            Console.WriteLine($"[DEBUG-VNPAY] TmnCode: {vnpTmnCode}");
            Console.WriteLine($"[DEBUG-VNPAY] HashSecret: {vnpHashSecret?.Substring(0, 3)}...");
            Console.WriteLine($"[DEBUG-VNPAY] ReturnUrl: {vnpReturnUrl}");


            if (string.IsNullOrEmpty(vnpUrl) || string.IsNullOrEmpty(vnpTmnCode) || string.IsNullOrEmpty(vnpHashSecret))
            {
                throw new Exception("Thiếu thông tin cấu hình VNPay. Vui lòng kiểm tra file appsettings.json");
            }


            var vnpay = new VnPayLibrary();


            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode);


            var amount = Convert.ToInt64(order.TotalAmount) * 100;
            vnpay.AddRequestData("vnp_Amount", amount.ToString()); 


            Console.WriteLine($"[DEBUG-VNPAY] OrderId: {order.Id}");
            Console.WriteLine($"[DEBUG-VNPAY] OrderNumber: {order.OrderNumber}");
            Console.WriteLine($"[DEBUG-VNPAY] TotalAmount: {order.TotalAmount:N0} VNĐ");
            Console.WriteLine($"[DEBUG-VNPAY] vnp_Amount (x100): {amount}");


            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            vnpay.AddRequestData("vnp_CreateDate", createDate);
            Console.WriteLine($"[DEBUG-VNPAY] CreateDate: {createDate}");

            vnpay.AddRequestData("vnp_CurrCode", "VND");
            

            var ipAddress = VnPayLibrary.GetIpAddress(_httpContextAccessor.HttpContext);
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            Console.WriteLine($"[DEBUG-VNPAY] IpAddr: {ipAddress}");
            
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang: {order.OrderNumber}");
            vnpay.AddRequestData("vnp_OrderType", "other"); 
            vnpay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);
            

            var expireDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");
            vnpay.AddRequestData("vnp_ExpireDate", expireDate);
            Console.WriteLine($"[DEBUG-VNPAY] ExpireDate: {expireDate}");
            

            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

            try
            {

                Console.WriteLine("[DEBUG-VNPAY] Bắt đầu tạo URL thanh toán...");
                string paymentUrl = vnpay.CreateRequestUrl(vnpUrl, vnpHashSecret);
                Console.WriteLine($"[DEBUG-VNPAY] URL thanh toán đã tạo: {paymentUrl}");
                
                return paymentUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG-VNPAY] Lỗi khi tạo URL: {ex.Message}");
                throw new Exception($"Lỗi khi tạo URL thanh toán: {ex.Message}", ex);
            }
        }

        public async Task<OrderDto> ProcessPaymentReturnAsync(IQueryCollection queryCollection)
        {
            var vnp_HashSecret = _configuration["Payment:VnPay:HashSecret"];
            var vnpay = new VnPayLibrary();


            foreach (var (key, value) in queryCollection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            var orderId = Guid.Parse(vnpay.GetResponseData("vnp_TxnRef"));
            var vnpayTranId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var vnp_SecureHash = queryCollection["vnp_SecureHash"];
            var vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount"));
            var vnp_BankCode = vnpay.GetResponseData("vnp_BankCode");
            

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            
            if (checkSignature)
            {

                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new Exception("Đơn hàng không tồn tại");
                }


                if (vnp_Amount / 100 != Convert.ToInt64(order.TotalAmount))
                {
                    throw new Exception("Số tiền thanh toán không hợp lệ");
                }


                if (order.OrderStatus != OrderStatus.Pending)
                {

                    return await GetOrderByIdAsync(orderId);
                }

                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {

                    order.PaymentTransactionId = vnpayTranId;
                    order.PaymentDate = DateTime.Now;
                    order.OrderStatus = OrderStatus.Processing;
                    
                    _unitOfWork.Repository<Order>().Update(order);
                    await _unitOfWork.CompleteAsync();


                    var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(order.CustomerId);
                    if (customer != null)
                    {

                        var cart = await _cartService.GetCartAsync(customer.UserId);
                        if (cart != null && cart.Items.Any())
                        {

                            var orderDetailRepository = _unitOfWork.Repository<OrderDetail>();
                            var orderedProductIds = await orderDetailRepository.Ts
                                .Where(od => od.OrderId == order.Id)
                                .Select(od => od.ProductId)
                                .ToListAsync();


                            foreach (var productId in orderedProductIds)
                            {
                                await _cartService.RemoveFromCartAsync(customer.UserId, productId);
                            }
                        }
                    }
                    
                    return await GetOrderByIdAsync(orderId);
                }
                else
                {

                    order.OrderStatus = OrderStatus.Cancelled;
                    order.Notes = $"{order.Notes}\nThanh toán thất bại. Mã lỗi: {vnp_ResponseCode}";
                    
                    _unitOfWork.Repository<Order>().Update(order);
                    await _unitOfWork.CompleteAsync();
                    
                    throw new Exception($"Thanh toán thất bại. Mã lỗi: {vnp_ResponseCode}");
                }
            }
            else
            {
                throw new Exception("Chữ ký không hợp lệ");
            }
        }
    }
} 
