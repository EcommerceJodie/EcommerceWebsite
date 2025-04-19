using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EcommerceWebsite.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartId = GetCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            return View(cart);
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            var cartId = GetCartId();
            var addToCartDto = new AddToCartDto
            {
                ProductId = productId,
                Quantity = quantity
            };

            await _cartService.AddToCartAsync(cartId, addToCartDto);

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid productId, int quantity)
        {
            if (quantity < 0)
            {
                return BadRequest();
            }

            var cartId = GetCartId();
            
            if (quantity == 0)
            {
                await _cartService.RemoveFromCartAsync(cartId, productId);
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng";
            }
            else
            {
                await _cartService.UpdateCartItemAsync(cartId, productId, quantity);
                TempData["SuccessMessage"] = "Giỏ hàng đã được cập nhật";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> RemoveItem(Guid productId)
        {
            var cartId = GetCartId();
            await _cartService.RemoveFromCartAsync(cartId, productId);
            
            TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var cartId = GetCartId();
            await _cartService.ClearCartAsync(cartId);
            
            TempData["SuccessMessage"] = "Giỏ hàng đã được xóa";
            return RedirectToAction("Index");
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MergeCarts()
        {
            var anonymousCartId = HttpContext.Request.Cookies["cart_id"];
            if (!string.IsNullOrEmpty(anonymousCartId))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _cartService.MergeCartsAsync(anonymousCartId, userId);
                

                HttpContext.Response.Cookies.Delete("cart_id");
                
                TempData["SuccessMessage"] = "Giỏ hàng của bạn đã được đồng bộ";
            }
            
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var cartId = GetCartId();
            var cart = await _cartService.GetCartAsync(cartId);
            return Json(new { count = cart.TotalItems });
        }


        private string GetCartId()
        {
            if (User.Identity.IsAuthenticated)
            {

                return User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {

                if (Request.Cookies.TryGetValue("cart_id", out string cartId) && !string.IsNullOrEmpty(cartId))
                {
                    return cartId;
                }


                cartId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append("cart_id", cartId, cookieOptions);
                return cartId;
            }
        }
    }
} 
