using Ecommerce.Core.DTOs;
using System;
using System.Threading.Tasks;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string cartId);
        Task<CartDto> GetOrCreateCartAsync(string customerId);
        Task<CartDto> AddToCartAsync(string cartId, AddToCartDto addToCartDto);
        Task<CartDto> UpdateCartItemAsync(string cartId, Guid productId, int quantity);
        Task<CartDto> RemoveFromCartAsync(string cartId, Guid productId);
        Task<bool> ClearCartAsync(string cartId);
        Task MergeCartsAsync(string anonymousCartId, string customerId);
    }
} 
