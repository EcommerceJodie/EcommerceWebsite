using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ecommerce.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly IRedisRepository _redisRepository;
        private readonly IProductRepository _productRepository;
        private const string CartPrefix = "cart:";
        private const string UserCartPrefix = "user_cart:";
        private const int CartExpiryDays = 30;

        public CartService(IRedisRepository redisRepository, IProductRepository productRepository)
        {
            _redisRepository = redisRepository;
            _productRepository = productRepository;
        }

        public async Task<CartDto> GetCartAsync(string cartId)
        {
            var key = $"{CartPrefix}{cartId}";
            var cartJson = await _redisRepository.GetAsync(key);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartDto { CartId = cartId, Items = new List<CartItemDto>() };
            }

            return JsonSerializer.Deserialize<CartDto>(cartJson);
        }

        public async Task<CartDto> GetOrCreateCartAsync(string customerId)
        {

            var userCartKey = $"{UserCartPrefix}{customerId}";
            var existingCartId = await _redisRepository.GetAsync(userCartKey);

            if (!string.IsNullOrEmpty(existingCartId))
            {

                return await GetCartAsync(existingCartId);
            }


            var newCartId = customerId;
            var newCart = new CartDto
            {
                CartId = newCartId,
                CustomerId = customerId,
                Items = new List<CartItemDto>()
            };


            await SaveCartAsync(newCart);
            await _redisRepository.SetAsync(userCartKey, newCartId, TimeSpan.FromDays(CartExpiryDays));

            return newCart;
        }

        public async Task<CartDto> AddToCartAsync(string cartId, AddToCartDto addToCartDto)
        {

            var product = await _productRepository.GetByIdAsync(addToCartDto.ProductId);
            
            if (product == null)
            {
                throw new ApplicationException("Sản phẩm không tồn tại");
            }


            var cart = await GetCartAsync(cartId);


            var existingItem = cart.Items.Find(i => i.ProductId == addToCartDto.ProductId);

            if (existingItem != null)
            {

                existingItem.Quantity += addToCartDto.Quantity;
            }
            else
            {

                string mainImageUrl = null;
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    var mainImage = product.ProductImages.FirstOrDefault(img => img.IsMainImage);
                    mainImageUrl = mainImage?.ImageUrl ?? product.ProductImages.FirstOrDefault()?.ImageUrl;
                }

                cart.Items.Add(new CartItemDto
                {
                    CartId = cartId,
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    ProductSlug = product.ProductSlug,
                    ProductImageUrl = mainImageUrl,
                    Quantity = addToCartDto.Quantity,
                    UnitPrice = product.ProductPrice,
                    DiscountPrice = product.ProductDiscountPrice
                });
            }


            await SaveCartAsync(cart);

            return cart;
        }

        public async Task<CartDto> UpdateCartItemAsync(string cartId, Guid productId, int quantity)
        {
            var cart = await GetCartAsync(cartId);
            var item = cart.Items.Find(i => i.ProductId == productId);

            if (item == null)
            {
                throw new ApplicationException("Sản phẩm không có trong giỏ hàng");
            }

            if (quantity <= 0)
            {

                return await RemoveFromCartAsync(cartId, productId);
            }


            item.Quantity = quantity;


            await SaveCartAsync(cart);

            return cart;
        }

        public async Task<CartDto> RemoveFromCartAsync(string cartId, Guid productId)
        {
            var cart = await GetCartAsync(cartId);
            
            cart.Items.RemoveAll(i => i.ProductId == productId);
            

            await SaveCartAsync(cart);

            return cart;
        }

        public async Task<bool> ClearCartAsync(string cartId)
        {
            var key = $"{CartPrefix}{cartId}";
            

            var emptyCart = new CartDto
            {
                CartId = cartId,
                CustomerId = (await GetCartAsync(cartId)).CustomerId,
                Items = new List<CartItemDto>()
            };
            

            return await SaveCartAsync(emptyCart);
        }

        public async Task MergeCartsAsync(string anonymousCartId, string customerId)
        {

            var anonymousCart = await GetCartAsync(anonymousCartId);
            
            if (anonymousCart.Items.Count == 0)
            {
                return; 
            }


            var userCart = await GetOrCreateCartAsync(customerId);


            foreach (var item in anonymousCart.Items)
            {
                var existingItem = userCart.Items.Find(i => i.ProductId == item.ProductId);
                
                if (existingItem != null)
                {

                    existingItem.Quantity += item.Quantity;
                }
                else
                {

                    userCart.Items.Add(item);
                }
            }


            await SaveCartAsync(userCart);
            

            var anonymousKey = $"{CartPrefix}{anonymousCartId}";
            await _redisRepository.DeleteAsync(anonymousKey);
        }

        private async Task<bool> SaveCartAsync(CartDto cart)
        {
            var key = $"{CartPrefix}{cart.CartId}";
            var cartJson = JsonSerializer.Serialize(cart);
            
            return await _redisRepository.SetAsync(key, cartJson, TimeSpan.FromDays(CartExpiryDays));
        }
    }
} 
