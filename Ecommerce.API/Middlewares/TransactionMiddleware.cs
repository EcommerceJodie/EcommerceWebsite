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

            if (!IsWriteOperation(context.Request.Method))
            {
                await _next(context);
                return;
            }

            try
            {

                await unitOfWork.BeginTransactionAsync();


                await _next(context);



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
