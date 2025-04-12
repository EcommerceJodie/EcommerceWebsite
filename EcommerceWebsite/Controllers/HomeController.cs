using Ecommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Ecommerce.Shared.Storage.Minio.Interfaces;

namespace Ecommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMinioService _minioService;

        public HomeController(ILogger<HomeController> logger, IMinioService minioService)
        {
            _logger = logger;
            _minioService = minioService;
        }

        public IActionResult Index()
        {
            return View();
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
        
        // Thêm hành động để kiểm tra kết nối MinIO
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
