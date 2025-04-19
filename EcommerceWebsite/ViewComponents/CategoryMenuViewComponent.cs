using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Web.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly IMenuConfigService _menuConfigService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryMenuViewComponent> _logger;

        public CategoryMenuViewComponent(
            IMenuConfigService menuConfigService, 
            ICategoryService categoryService,
            ILogger<CategoryMenuViewComponent> logger)
        {
            _menuConfigService = menuConfigService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isMainMenu = true)
        {
            try
            {

                var menuTree = await _menuConfigService.GetMenuTreeAsync();
                

                var visibleMenus = menuTree
                    .Where(m => m.IsMainMenu == isMainMenu && m.IsVisible)
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();
                
                return View(visibleMenus);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải menu danh mục");

                return View(new List<MenuConfigDto>());
            }
        }
    }
} 
