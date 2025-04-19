using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface IPaymentTransactionService
    {
        Task<PaymentTransactionDto> CreateTransactionAsync(CreatePaymentTransactionDto createDto);
        Task<PaymentTransactionDto> GetTransactionByIdAsync(Guid id);
        Task<IEnumerable<PaymentTransactionDto>> GetTransactionsByOrderIdAsync(Guid orderId);
        Task<PaymentTransactionDto> ProcessVnPayResponseAsync(IQueryCollection queryCollection);
    }
} 
