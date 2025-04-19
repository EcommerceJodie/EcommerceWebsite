using Ecommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.DTOs;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Ecommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMinioService _minioService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(
            ILogger<HomeController> logger, 
            IMinioService minioService,
            IProductService productService,
            ICategoryService categoryService)
        {
            _logger = logger;
            _minioService = minioService;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string sortBy = "createdAt", bool sortDesc = true, Guid? categoryId = null, string searchTerm = "")
        {
            try
            {
                ViewData["Title"] = "Trang chủ";
                

                var featuredProducts = await _productService.GetFeaturedProductsAsync();
                ViewBag.FeaturedProducts = featuredProducts.Take(6).ToList();
                

                var query = new ProductQueryDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDesc = sortDesc,
                    CategoryId = categoryId,
                    SearchTerm = searchTerm,
                    InStock = true
                };
                
                var products = await _productService.GetPagedProductsAsync(query);
                

                var categories = await _categoryService.GetAllCategoriesAsync();
                

                ViewBag.FeaturedCategories = categories.Take(6).ToList();
                ViewBag.Categories = categories; 
                

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = products.TotalPages;
                ViewBag.TotalItems = products.TotalCount;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;
                ViewBag.CategoryId = categoryId;
                ViewBag.SearchTerm = searchTerm;
                
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chủ");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public async Task<IActionResult> Products(int page = 1, int pageSize = 12, string sortBy = "createdAt", bool sortDesc = true, Guid? categoryId = null, string searchTerm = "")
        {
            try
            {
                ViewData["Title"] = "Danh sách sản phẩm";
                
                var query = new ProductQueryDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDesc = sortDesc,
                    CategoryId = categoryId,
                    SearchTerm = searchTerm,
                    InStock = true
                };
                
                var products = await _productService.GetPagedProductsAsync(query);
                

                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = products.TotalPages;
                ViewBag.TotalItems = products.TotalCount;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;
                ViewBag.CategoryId = categoryId;
                ViewBag.SearchTerm = searchTerm;
                
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách sản phẩm");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        

        public IActionResult TestMinioConnection()
        {
            try
            {
                bool isConnected = _minioService.IsMinioConnectionActive();
                
                var result = new
                {
                    isConnected = isConnected,
                    message = isConnected 
                        ? "Kết nối đến MinIO thành công!" 
                        : "Không thể kết nối đến MinIO. Vui lòng kiểm tra lại cấu hình và đảm bảo máy chủ MinIO đang chạy."
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra kết nối MinIO");
                return Json(new 
                { 
                    isConnected = false, 
                    message = $"Lỗi: {ex.Message}", 
                    details = ex.ToString() 
                });
            }
        }
    }
}
