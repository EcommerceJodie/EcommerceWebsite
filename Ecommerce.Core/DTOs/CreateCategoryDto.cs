using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.DTOs
{
    public class CreateCategoryDto
    {
        public CreateCategoryDto()
        {
            CategoryImageUrl = string.Empty;
            CategoryDescription = string.Empty;
            CategorySlug = string.Empty;
            IsActive = true;
            DisplayOrder = 0;
        }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string CategoryName { get; set; }
        
        [StringLength(500, ErrorMessage = "Mô tả danh mục không được vượt quá 500 ký tự")]
        public string CategoryDescription { get; set; }
        
        [StringLength(100, ErrorMessage = "Slug không được vượt quá 100 ký tự")]
        public string CategorySlug { get; set; }
        
        [StringLength(255, ErrorMessage = "URL hình ảnh không được vượt quá 255 ký tự")]
        public string CategoryImageUrl { get; set; }
        
        public IFormFile CategoryImage { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 1000")]
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 