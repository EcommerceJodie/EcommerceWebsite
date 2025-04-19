using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class MenuConfigsController : ControllerBase
    {
        private readonly IMenuConfigService _menuConfigService;
        private readonly ILogger<MenuConfigsController> _logger;

        public MenuConfigsController(IMenuConfigService menuConfigService, ILogger<MenuConfigsController> logger)
        {
            _menuConfigService = menuConfigService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuConfigDto>>> GetMenuConfigs()
        {
            try
            {
                var menuConfigs = await _menuConfigService.GetAllMenuConfigsAsync();
                return Ok(menuConfigs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cấu hình menu");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpGet("tree")]
        public async Task<ActionResult<IEnumerable<MenuConfigDto>>> GetMenuConfigTree()
        {
            try
            {
                var menuTree = await _menuConfigService.GetMenuTreeAsync();
                return Ok(menuTree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cấu trúc cây menu");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpGet("root")]
        public async Task<ActionResult<IEnumerable<MenuConfigDto>>> GetRootMenuConfigs([FromQuery] bool isMainMenu = true)
        {
            try
            {
                var menuConfigs = await _menuConfigService.GetRootMenuConfigsAsync(isMainMenu);
                return Ok(menuConfigs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cấu hình menu gốc");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpGet("visible")]
        public async Task<ActionResult<IEnumerable<MenuConfigDto>>> GetVisibleMenuConfigs([FromQuery] bool isMainMenu = true)
        {
            try
            {
                var menuConfigs = await _menuConfigService.GetVisibleMenuConfigsAsync(isMainMenu);
                return Ok(menuConfigs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách cấu hình menu hiển thị");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpGet("by-parent/{parentId}")]
        public async Task<ActionResult<IEnumerable<MenuConfigDto>>> GetMenuConfigsByParent(Guid? parentId)
        {
            try
            {
                var menuConfigs = await _menuConfigService.GetMenuConfigsByParentIdAsync(parentId);
                return Ok(menuConfigs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách cấu hình menu con cho parent ID: {parentId}");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MenuConfigDto>> GetMenuConfig(Guid id)
        {
            try
            {
                var menuConfig = await _menuConfigService.GetMenuConfigByIdAsync(id);
                return Ok(menuConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy cấu hình menu với ID: {id}");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<MenuConfigDto>> GetMenuConfigByCategory(Guid categoryId, [FromQuery] bool isMainMenu = true)
        {
            try
            {
                var menuConfig = await _menuConfigService.GetByCategoryIdAsync(categoryId, isMainMenu);
                if (menuConfig == null)
                {
                    return NotFound($"Không tìm thấy cấu hình menu cho danh mục với ID: {categoryId}");
                }
                return Ok(menuConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy cấu hình menu cho danh mục với ID: {categoryId}");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpPost]
        public async Task<ActionResult<MenuConfigDto>> CreateMenuConfig([FromBody] CreateMenuConfigDto createMenuConfigDto)
        {
            try
            {
                var menuConfig = await _menuConfigService.CreateMenuConfigAsync(createMenuConfigDto);
                return CreatedAtAction(nameof(GetMenuConfig), new { id = menuConfig.Id }, menuConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo cấu hình menu");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuConfig(Guid id, [FromBody] UpdateMenuConfigDto updateMenuConfigDto)
        {
            if (id != updateMenuConfigDto.Id)
            {
                return BadRequest("ID trong đường dẫn không khớp với ID trong dữ liệu");
            }

            try
            {
                var menuConfig = await _menuConfigService.UpdateMenuConfigAsync(updateMenuConfigDto);
                return Ok(menuConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật cấu hình menu với ID: {id}");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuConfig(Guid id)
        {
            try
            {
                await _menuConfigService.DeleteMenuConfigAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa cấu hình menu với ID: {id}");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn");
            }
        }
    }
} 
