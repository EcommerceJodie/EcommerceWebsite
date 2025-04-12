using Ecommerce.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ecommerce.API.Middlewares
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            // Chỉ xử lý giao dịch cho các API endpoint có thể thay đổi dữ liệu
            if (!IsWriteOperation(context.Request.Method))
            {
                await _next(context);
                return;
            }

            try
            {
                // Bắt đầu giao dịch
                await unitOfWork.BeginTransactionAsync();

                // Gọi middleware tiếp theo trong pipeline
                await _next(context);

                // Nếu không có lỗi và StatusCode trong phạm vi thành công (2xx),
                // commit giao dịch
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    await unitOfWork.CommitTransactionAsync();
                }
                else
                {
                    await unitOfWork.RollbackTransactionAsync();
                }
            }
            catch
            {
                // Rollback giao dịch khi có lỗi
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private bool IsWriteOperation(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
        }
    }
} 