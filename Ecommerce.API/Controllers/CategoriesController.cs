using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Core.DTOs;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }


        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetActiveCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var activeCategories = categories.FindAll(c => c.IsActive);
            return Ok(activeCategories);
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }


        [HttpGet("{id}/image-url")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GetCategoryImagePresignedUrl(Guid id, [FromQuery] int expiryMinutes = 30)
        {
            var presignedUrl = await _categoryService.GetCategoryImagePresignedUrlAsync(id, expiryMinutes);
            return Ok(new { url = presignedUrl });
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromForm] CreateCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCategory = await _categoryService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromForm] UpdateCategoryDto categoryDto, [FromQuery] bool keepExistingImage = false)
        {
            try 
            {
                if (id != categoryDto.Id)
                {
                    return BadRequest("ID trong đường dẫn không khớp với ID trong dữ liệu");
                }
                
                // Log để debug
                Console.WriteLine($"keepExistingImage: {keepExistingImage}");
                Console.WriteLine($"CategoryImageUrl trước khi xử lý: {categoryDto.CategoryImageUrl}");
                
                // Kiểm tra nếu keepExistingImage = true, lấy lại thông tin danh mục hiện tại
                if (keepExistingImage)
                {
                    try
                    {
                        // Lấy thông tin danh mục hiện tại và sử dụng lại URL ảnh
                        var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
                        
                        if (!string.IsNullOrEmpty(existingCategory.CategoryImageUrl))
                        {
                            // Lấy URL cơ bản mà không có tham số query
                            string imageUrl = existingCategory.CategoryImageUrl;
                            if (imageUrl.Contains("?"))
                            {
                                imageUrl = imageUrl.Substring(0, imageUrl.IndexOf("?"));
                            }
                            
                            categoryDto.CategoryImageUrl = imageUrl;
                            Console.WriteLine($"Sử dụng lại CategoryImageUrl (đã cắt): {categoryDto.CategoryImageUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi để debug
                        Console.WriteLine($"Lỗi khi lấy danh mục hiện tại: {ex.Message}");
                        Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        // Tiếp tục xử lý với thông tin hiện tại
                    }
                }

                var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật danh mục: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { 
                    status = "Error", 
                    message = "Đã xảy ra lỗi trong quá trình xử lý yêu cầu", 
                    details = ex.Message 
                });
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
} 
