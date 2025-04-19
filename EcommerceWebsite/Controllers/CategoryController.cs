using System;
using System.Threading.Tasks;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Web.Models;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ecommerce.Core.DTOs;

namespace Ecommerce.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMenuConfigService _menuConfigService;
        private readonly ILogger<CategoryController> _logger;
        private readonly IProductService _productService;

        public CategoryController(
            ICategoryService categoryService,
            IMenuConfigService menuConfigService,
            IProductService productService,
            ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _menuConfigService = menuConfigService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị danh sách danh mục");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hiển thị chi tiết danh mục có ID: {id}");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> Products(Guid id, int page = 1, int pageSize = 12, string sortBy = "CreatedAt", bool sortDesc = true, string searchTerm = "")
        {
            try
            {

                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }


                var pagination = new PaginationRequestDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDesc = sortDesc,
                    SearchTerm = searchTerm
                };


                var products = await _productService.GetProductsByCategoryAsync(id, pagination);


                ViewBag.CurrentPage = products.PageNumber;
                ViewBag.TotalPages = products.TotalPages;
                ViewBag.TotalItems = products.TotalCount;
                ViewBag.PageSize = products.PageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDesc = sortDesc;
                ViewBag.CategoryId = id;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Category = category;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hiển thị sản phẩm theo danh mục có ID: {id}");
                return RedirectToAction("Error", "Home");
            }
        }

        public async Task<IActionResult> MenuCategories()
        {
            try
            {

                var menuTree = await _menuConfigService.GetMenuTreeAsync();
                

                var mainMenus = menuTree
                    .Where(m => m.IsMainMenu && m.IsVisible)
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();
                
                return View(mainMenus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải cấu trúc menu cho header");
                return RedirectToAction("Error", "Home");
            }
        }
    }
} 
