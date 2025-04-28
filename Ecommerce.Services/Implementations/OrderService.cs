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
using Microsoft.Extensions.Logging;

namespace Ecommerce.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            ICartService cartService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        // Các phương thức mới để hỗ trợ tạo đơn hàng từ admin
        public async Task<OrderDto> CreateOrderFromAdminAsync(AdminCreateOrderDto createOrderDto, string adminUserId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Lấy thông tin khách hàng từ ID
                var customerRepository = _unitOfWork.Repository<Customer>();
                var customer = await customerRepository.GetByIdAsync(createOrderDto.CustomerId);
                if (customer == null)
                {
                    throw new Exception($"Không tìm thấy khách hàng với ID: {createOrderDto.CustomerId}");
                }

                // Lấy thông tin sản phẩm và xác nhận tồn tại
                var productRepository = _unitOfWork.Repository<Product>();
                var orderItems = new List<(Product Product, int Quantity)>();

                foreach (var item in createOrderDto.OrderItems)
                {
                    var product = await productRepository.Ts
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                    {
                        throw new Exception($"Không tìm thấy sản phẩm với ID: {item.ProductId}");
                    }
                    
                    orderItems.Add((product, item.Quantity));
                }

                // Tính toán tổng tiền
                decimal subtotal = orderItems.Sum(item => item.Product.ProductPrice * item.Quantity);
                decimal totalAmount = subtotal;

                // Tạo đơn hàng mới
                var orderNumber = $"ADM{DateTime.Now.ToString("yyyyMMddHHmmss")}{customer.Id.ToString().Substring(0, 8)}";
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    TotalAmount = totalAmount,
                    OrderStatus = OrderStatus.Processing, // Đơn hàng từ admin mặc định là đang xử lý
                    ShippingAddress = createOrderDto.ShippingAddress ?? customer.Address,
                    ShippingCity = customer.City,
                    ShippingPostalCode = customer.PostalCode,
                    ShippingCountry = customer.Country,
                    PaymentMethod = "CASH", // Mặc định là tiền mặt
                    CustomerId = customer.Id,
                    Notes = createOrderDto.Note,
                    CreatedBy = adminUserId // Lưu ID của admin tạo đơn
                };

                _unitOfWork.Repository<Order>().Add(order);
                await _unitOfWork.CompleteAsync();

                // Thêm chi tiết đơn hàng
                var orderDetailRepository = _unitOfWork.Repository<OrderDetail>();
                foreach (var (product, quantity) in orderItems)
                {
                    var unitPrice = product.ProductDiscountPrice ?? product.ProductPrice;
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Discount = 0, // Không có giảm giá
                        Subtotal = unitPrice * quantity
                    };

                    orderDetailRepository.Add(orderDetail);
                }

                // Tạo giao dịch thanh toán tiền mặt
                var paymentTransaction = new PaymentTransaction
                {
                    OrderId = order.Id,
                    TransactionCode = $"CASH{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                    TransactionStatus = "Success",
                    ResponseCode = "00",
                    Status = PaymentStatus.Completed,
                    PaymentMethod = "CASH",
                    Amount = totalAmount,
                    PaymentTime = DateTime.Now,
                    Notes = "Thanh toán tiền mặt tại quầy",
                    BankCode = "CASH", // Thêm giá trị mặc định cho BankCode
                    CardType = "CASH", // Thêm giá trị mặc định cho CardType
                    RawData = "{\"PaymentMethod\":\"CASH\",\"Amount\":" + totalAmount + "}" // Thêm giá trị JSON cho RawData
                };

                _unitOfWork.Repository<PaymentTransaction>().Add(paymentTransaction);
                
                // Cập nhật trạng thái đơn hàng
                order.OrderStatus = OrderStatus.Processing;
                order.PaymentDate = DateTime.Now;
                order.PaymentTransactionId = paymentTransaction.Id.ToString();
                _unitOfWork.Repository<Order>().Update(order);

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await GetOrderByIdAsync(order.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Lỗi khi tạo đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<List<CustomerSearchResultDto>> SearchCustomersByPhoneAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 3)
            {
                throw new ArgumentException("Số điện thoại tìm kiếm phải có ít nhất 3 ký tự");
            }

            try
            {
                // Lấy danh sách khách hàng từ DB trước, rồi mới thực hiện chuyển đổi sang DTO
                var customers = await _unitOfWork.Repository<Customer>().Ts
                    .Where(c => c.PhoneNumber.Contains(phoneNumber))
                    .Take(10)
                    .ToListAsync();

                // Sau đó mới map sang DTO
                return customers.Select(c => new CustomerSearchResultDto
                {
                    Id = c.Id,
                    FullName = $"{c.FirstName} {c.LastName}".Trim(),
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email ?? "",
                    Address = c.Address,
                    City = c.City,
                    Country = c.Country
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm khách hàng: {Message}", ex.Message);
                throw new Exception($"Lỗi khi tìm kiếm khách hàng: {ex.Message}", ex);
            }
        }
        
        // Helper method để tạo địa chỉ đầy đủ
        private string FormatAddress(string address, string city, string country)
        {
            var addressParts = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(address))
                addressParts.Add(address);
                
            if (!string.IsNullOrWhiteSpace(city))
                addressParts.Add(city);
                
            if (!string.IsNullOrWhiteSpace(country))
                addressParts.Add(country);
                
            return string.Join(", ", addressParts);
        }

        public async Task<OrderBillDto> GetOrderBillAsync(Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order>().Ts
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception($"Không tìm thấy đơn hàng với ID: {orderId}");
            }

            var userRepository = _unitOfWork.Repository<ApplicationUser>();
            var createdBy = !string.IsNullOrEmpty(order.CreatedBy) 
                ? await userRepository.Ts.FirstOrDefaultAsync(u => u.Id == order.CreatedBy) 
                : null;

            var items = new List<OrderDetailDto>();
            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product;
                string imageUrl = "";
                
                // Lấy ảnh sản phẩm nếu có
                var productWithImages = await _unitOfWork.Repository<Product>().Ts
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);
                
                if (productWithImages?.ProductImages?.Any() == true)
                {
                    imageUrl = productWithImages.ProductImages.FirstOrDefault()?.ImageUrl ?? "";
                }

                items.Add(new OrderDetailDto
                {
                    Id = detail.Id,
                    ProductId = detail.ProductId,
                    ProductName = product.ProductName,
                    ProductImageUrl = imageUrl,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Discount = detail.Discount,
                    Subtotal = detail.Subtotal
                });
            }

            // Tính toán giá trị đơn hàng
            decimal subtotal = items.Sum(i => i.Subtotal);
            decimal discountTotal = items.Sum(i => i.Discount * i.Quantity);
            
            // Ước tính VAT (10% nếu có VAT)
            decimal vatAmount = 0;
            if (order.OrderNumber.StartsWith("ADM")) // Đơn hàng từ admin
            {
                if (subtotal * 1.1m == order.TotalAmount)
                {
                    vatAmount = subtotal * 0.1m;
                }
                else
                {
                    vatAmount = order.TotalAmount - subtotal;
                }
            }

            return new OrderBillDto
            {
                OrderNumber = order.OrderNumber,
                CreatedAt = order.CreatedAt,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                CustomerPhone = order.Customer.PhoneNumber,
                CustomerAddress = $"{order.ShippingAddress}, {order.ShippingCity}, {order.ShippingCountry}",
                PaymentMethod = order.PaymentMethod,
                Subtotal = subtotal,
                VatAmount = vatAmount,
                DiscountTotal = discountTotal,
                TotalAmount = order.TotalAmount,
                Items = items,
                Notes = order.Notes,
                CreatedBy = createdBy != null ? $"{createdBy.FirstName} {createdBy.LastName}" : "Hệ thống"
            };
        }

        public async Task<OrderListResponseDto> GetOrdersAsync(OrderFilterDto filterDto)
        {
            try
            {
                var orderRepository = _unitOfWork.Repository<Order>();
                
                // Bắt đầu truy vấn
                var query = orderRepository.Ts
                    .Include(o => o.Customer)
                    .AsQueryable();
                
                // Áp dụng các bộ lọc
                if (filterDto.FromDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= filterDto.FromDate.Value.Date);
                }
                
                if (filterDto.ToDate.HasValue)
                {
                    // Bao gồm cả ngày kết thúc
                    var endDate = filterDto.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.CreatedAt <= endDate);
                }
                
                if (filterDto.Status.HasValue)
                {
                    query = query.Where(o => o.OrderStatus == filterDto.Status.Value);
                }
                
                if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
                {
                    var searchTerm = filterDto.SearchTerm.Trim().ToLower();
                    query = query.Where(o => 
                        o.OrderNumber.ToLower().Contains(searchTerm) ||
                        (o.Customer.FirstName + " " + o.Customer.LastName).ToLower().Contains(searchTerm) ||
                        o.Customer.PhoneNumber.Contains(searchTerm)
                    );
                }
                
                // Tính tổng số đơn hàng phù hợp với điều kiện lọc
                var totalCount = await query.CountAsync();
                
                // Áp dụng sắp xếp
                query = ApplySorting(query, filterDto.SortBy, filterDto.IsDescending);
                
                // Áp dụng phân trang
                var pageSize = filterDto.PageSize > 0 ? (filterDto.PageSize <= 50 ? filterDto.PageSize : 50) : 10;
                var pageNumber = filterDto.PageNumber > 0 ? filterDto.PageNumber : 1;
                var skip = (pageNumber - 1) * pageSize;
                
                var orders = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
                
                // Chuyển đổi sang DTO
                var orderDtos = orders.Select(o => new OrderListDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CreatedAt = o.CreatedAt,
                    CustomerId = o.CustomerId,
                    CustomerName = $"{o.Customer?.FirstName} {o.Customer?.LastName}".Trim(),
                    PhoneNumber = o.Customer?.PhoneNumber,
                    TotalAmount = o.TotalAmount,
                    OrderStatus = o.OrderStatus,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentDate.HasValue ? "Đã thanh toán" : "Chưa thanh toán",
                    Note = o.Notes
                }).ToList();
                
                // Tạo kết quả phân trang
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                
                return new OrderListResponseDto
                {
                    Orders = orderDtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng: {Message}", ex.Message);
                throw new Exception($"Lỗi khi lấy danh sách đơn hàng: {ex.Message}", ex);
            }
        }
        
        private IQueryable<Order> ApplySorting(IQueryable<Order> query, string sortBy, bool isDescending)
        {
            switch (sortBy?.ToLower())
            {
                case "ordernumber":
                    return isDescending 
                        ? query.OrderByDescending(o => o.OrderNumber) 
                        : query.OrderBy(o => o.OrderNumber);
                case "customername":
                    return isDescending 
                        ? query.OrderByDescending(o => o.Customer.LastName).ThenByDescending(o => o.Customer.FirstName) 
                        : query.OrderBy(o => o.Customer.LastName).ThenBy(o => o.Customer.FirstName);
                case "totalamount":
                    return isDescending 
                        ? query.OrderByDescending(o => o.TotalAmount) 
                        : query.OrderBy(o => o.TotalAmount);
                case "status":
                    return isDescending 
                        ? query.OrderByDescending(o => o.OrderStatus) 
                        : query.OrderBy(o => o.OrderStatus);
                default:
                    // Mặc định sắp xếp theo CreatedAt
                    return isDescending 
                        ? query.OrderByDescending(o => o.CreatedAt) 
                        : query.OrderBy(o => o.CreatedAt);
            }
        }
    }
} 
