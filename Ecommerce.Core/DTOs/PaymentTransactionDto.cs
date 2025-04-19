using System;
using Ecommerce.Core.Models.Enums;

namespace Ecommerce.Core.DTOs
{
    public class PaymentTransactionDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        
        public string TransactionCode { get; set; }
        public string TransactionStatus { get; set; }
        public string ResponseCode { get; set; }
        public PaymentStatus Status { get; set; }
        
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string BankCode { get; set; }
        public string CardType { get; set; }
        
        public DateTime PaymentTime { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string Notes { get; set; }
    }
    
    public class CreatePaymentTransactionDto
    {
        public Guid OrderId { get; set; }
        public string TransactionCode { get; set; }
        public string TransactionStatus { get; set; }
        public string ResponseCode { get; set; }
        public PaymentStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string BankCode { get; set; }
        public string CardType { get; set; }
        public DateTime PaymentTime { get; set; }
        public string RawData { get; set; }
        public string Notes { get; set; }
    }
} 
