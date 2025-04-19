using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
using System.Globalization;

namespace Ecommerce.Services.Implementations
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        
        public PaymentTransactionService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }
        
        public async Task<PaymentTransactionDto> CreateTransactionAsync(CreatePaymentTransactionDto createDto)
        {
            var transaction = new PaymentTransaction
            {
                OrderId = createDto.OrderId,
                TransactionCode = createDto.TransactionCode,
                TransactionStatus = createDto.TransactionStatus,
                ResponseCode = createDto.ResponseCode,
                Status = createDto.Status,
                PaymentMethod = createDto.PaymentMethod,
                Amount = createDto.Amount,
                BankCode = createDto.BankCode,
                CardType = createDto.CardType,
                PaymentTime = createDto.PaymentTime,
                RawData = createDto.RawData,
                Notes = createDto.Notes
            };
            
            _unitOfWork.Repository<PaymentTransaction>().Add(transaction);
            await _unitOfWork.CompleteAsync();
            
            return await GetTransactionByIdAsync(transaction.Id);
        }
        
        public async Task<PaymentTransactionDto> GetTransactionByIdAsync(Guid id)
        {
            var transaction = await _unitOfWork.Repository<PaymentTransaction>().Ts
                .Include(t => t.Order)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (transaction == null)
            {
                return null;
            }
            
            return new PaymentTransactionDto
            {
                Id = transaction.Id,
                OrderId = transaction.OrderId,
                OrderNumber = transaction.Order?.OrderNumber,
                TransactionCode = transaction.TransactionCode,
                TransactionStatus = transaction.TransactionStatus,
                ResponseCode = transaction.ResponseCode,
                Status = transaction.Status,
                PaymentMethod = transaction.PaymentMethod,
                Amount = transaction.Amount,
                BankCode = transaction.BankCode,
                CardType = transaction.CardType,
                PaymentTime = transaction.PaymentTime,
                CreatedAt = transaction.CreatedAt,
                Notes = transaction.Notes
            };
        }
        
        public async Task<IEnumerable<PaymentTransactionDto>> GetTransactionsByOrderIdAsync(Guid orderId)
        {
            var transactions = await _unitOfWork.Repository<PaymentTransaction>().Ts
                .Include(t => t.Order)
                .Where(t => t.OrderId == orderId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            
            return transactions.Select(t => new PaymentTransactionDto
            {
                Id = t.Id,
                OrderId = t.OrderId,
                OrderNumber = t.Order?.OrderNumber,
                TransactionCode = t.TransactionCode,
                TransactionStatus = t.TransactionStatus,
                ResponseCode = t.ResponseCode,
                Status = t.Status,
                PaymentMethod = t.PaymentMethod,
                Amount = t.Amount,
                BankCode = t.BankCode,
                CardType = t.CardType,
                PaymentTime = t.PaymentTime,
                CreatedAt = t.CreatedAt,
                Notes = t.Notes
            });
        }
        
        public async Task<PaymentTransactionDto> ProcessVnPayResponseAsync(IQueryCollection queryCollection)
        {
            var vnp_HashSecret = _configuration["Payment:VnPay:HashSecret"];
            var vnpay = new VnPayLibrary();
            var rawData = JsonSerializer.Serialize(queryCollection);
            

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
            var vnp_CardType = vnpay.GetResponseData("vnp_CardType");
            var vnp_PayDate = vnpay.GetResponseData("vnp_PayDate");
            

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
            
            if (!checkSignature)
            {
                throw new Exception("Chữ ký không hợp lệ");
            }
            

            var orderRepository = _unitOfWork.Repository<Order>();
            var order = await orderRepository.GetByIdAsync(orderId);
            
            if (order == null)
            {
                throw new Exception("Đơn hàng không tồn tại");
            }
            

            var paymentStatus = PaymentStatus.Pending;
            if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
            {
                paymentStatus = PaymentStatus.Completed;
            }
            else
            {
                paymentStatus = PaymentStatus.Failed;
            }
            

            var createDto = new CreatePaymentTransactionDto
            {
                OrderId = orderId,
                TransactionCode = vnpayTranId,
                TransactionStatus = vnp_TransactionStatus,
                ResponseCode = vnp_ResponseCode,
                Status = paymentStatus,
                PaymentMethod = "VNPAY",
                Amount = vnp_Amount / 100, 
                BankCode = vnp_BankCode,
                CardType = vnp_CardType,
                PaymentTime = DateTime.ParseExact(vnp_PayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
                RawData = rawData,
                Notes = paymentStatus == PaymentStatus.Completed ? "Thanh toán thành công" : "Thanh toán thất bại"
            };
            

            var transaction = await CreateTransactionAsync(createDto);
            

            if (paymentStatus == PaymentStatus.Completed)
            {
                order.OrderStatus = OrderStatus.Processing;
                order.PaymentDate = DateTime.Now;
                order.PaymentTransactionId = transaction.Id.ToString();
                orderRepository.Update(order);
                await _unitOfWork.CompleteAsync();
            }
            
            return transaction;
        }
    }
} 
