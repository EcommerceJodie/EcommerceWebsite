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
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] bool sortDesc = true,
            [FromQuery] string searchTerm = "",
            [FromQuery] Guid? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? isFeatured = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] string status = null)
        {
            var queryDto = new ProductQueryDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDesc = sortDesc,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                IsFeatured = isFeatured,
                InStock = inStock,
                Status = status
            };
            
            var products = await _productService.GetPagedProductsAsync(queryDto);
            return Ok(products);
        }


        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }


        [HttpGet("Category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProductsByCategory(
            Guid categoryId, 
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] bool sortDesc = true,
            [FromQuery] string searchTerm = "")
        {
            var pagination = new PaginationRequestDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDesc = sortDesc,
                SearchTerm = searchTerm
            };
            
            var products = await _productService.GetProductsByCategoryAsync(categoryId, pagination);
            return Ok(products);
        }


        [HttpGet("Category/{categoryId}/all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsByCategory(Guid categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }


        [HttpGet("Featured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeaturedProducts()
        {
            var products = await _productService.GetFeaturedProductsAsync();
            return Ok(products);
        }


        [HttpGet("Images/{imageId}/presigned-url")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetProductImagePresignedUrl(Guid imageId, [FromQuery] int expiryMinutes = 30)
        {
            var presignedUrl = await _productService.GetProductImagePresignedUrlAsync(imageId, expiryMinutes);
            return Ok(new { url = presignedUrl });
        }
        

        [HttpGet("{productId}/main-image-url")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetProductMainImagePresignedUrl(Guid productId, [FromQuery] int expiryMinutes = 30)
        {
            var presignedUrl = await _productService.GetProductMainImagePresignedUrlAsync(productId, expiryMinutes);
            return Ok(new { url = presignedUrl });
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromForm] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Gán ID từ URL vào DTO
            updateProductDto.Id = id;

            await _productService.UpdateProductAsync(updateProductDto);
            return NoContent();
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        
        [HttpPost("batch")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMultipleProducts([FromBody] DeleteMultipleProductsDto request)
        {
            if (request == null || request.ProductIds == null || !request.ProductIds.Any())
            {
                return BadRequest("Danh sách ID sản phẩm cần xóa không được để trống");
            }
            
            var result = await _productService.DeleteMultipleProductsAsync(request.ProductIds);
            
            if (result)
            {
                return NoContent();
            }
            
            return BadRequest("Không thể xóa sản phẩm");
        }
        
        [HttpPost("{id}/duplicate")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> DuplicateProduct(Guid id, [FromBody] DuplicateProductDto duplicateDto)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("ID sản phẩm không hợp lệ");
            }
            
            // Gán ID từ URL vào DTO
            duplicateDto.SourceProductId = id;
            
            var duplicatedProduct = await _productService.DuplicateProductAsync(duplicateDto);
            return CreatedAtAction(nameof(GetProduct), new { id = duplicatedProduct.Id }, duplicatedProduct);
        }

        [HttpPost("{productId}/Images")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductImageDto>> AddProductImage(Guid productId, [FromForm] AddProductImageDto imageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var productImage = await _productService.AddProductImageAsync(productId, imageDto);
            return CreatedAtAction(nameof(GetProduct), new { id = productId }, productImage);
        }
        

        [HttpDelete("Images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProductImage(Guid imageId)
        {
            await _productService.DeleteProductImageAsync(imageId);
            return NoContent();
        }
        

        [HttpPut("Images/{imageId}/SetMain")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetMainProductImage(Guid imageId)
        {
            if (imageId == Guid.Empty)
            {
                return BadRequest("ID hình ảnh không hợp lệ");
            }

            await _productService.SetMainProductImageAsync(imageId);
            return NoContent();
        }
    }
} 
