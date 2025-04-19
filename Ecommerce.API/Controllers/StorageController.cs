using System;
using System.Threading.Tasks;
using Ecommerce.Shared.Storage.Minio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IMinioService _minioService;
        private readonly ILogger<StorageController> _logger;

        public StorageController(IMinioService minioService, ILogger<StorageController> logger)
        {
            _minioService = minioService;
            _logger = logger;
        }


        [HttpPost("reapply-bucket-policy")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReapplyBucketPolicy()
        {
            try
            {
                _logger.LogInformation("Áp dụng lại chính sách bucket");
                var result = await _minioService.ReapplyBucketPolicyAsync();
                
                if (result)
                {
                    return Ok(new { message = "Đã áp dụng lại chính sách bucket thành công" });
                }
                else
                {
                    return StatusCode(500, new { message = "Không thể áp dụng lại chính sách bucket" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi áp dụng lại chính sách bucket");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }
        

        [HttpPost("emergency-fix")]
        [AllowAnonymous] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmergencyFixBucketPolicy()
        {
            try
            {
                _logger.LogWarning("KHẨN CẤP: Áp dụng lại chính sách bucket không cần xác thực");
                var result = await _minioService.ReapplyBucketPolicyAsync();
                
                if (result)
                {
                    return Ok(new { message = "Đã áp dụng lại chính sách bucket thành công (khẩn cấp)" });
                }
                else
                {
                    return StatusCode(500, new { message = "Không thể áp dụng lại chính sách bucket (khẩn cấp)" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi áp dụng lại chính sách bucket (khẩn cấp)");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }


        [HttpGet("check-connection")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CheckConnection()
        {
            try
            {
                var isConnected = _minioService.IsMinioConnectionActive();
                return Ok(new { isConnected });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra kết nối MinIO");
                return StatusCode(500, new { message = $"Lỗi: {ex.Message}" });
            }
        }
    }
} 
