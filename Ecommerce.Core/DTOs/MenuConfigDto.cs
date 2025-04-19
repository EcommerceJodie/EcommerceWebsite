using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.DTOs
{
    public class MenuConfigDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategorySlug { get; set; }
        public string CategoryImageUrl { get; set; }
        public bool IsVisible { get; set; } = true;
        public int DisplayOrder { get; set; }
        public string CustomName { get; set; }
        public string Icon { get; set; }
        public bool IsMainMenu { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        

        public Guid? ParentId { get; set; }
        public List<MenuConfigDto> Children { get; set; } = new List<MenuConfigDto>();
    }

    public class CreateMenuConfigDto
    {
        [Required(ErrorMessage = "ID danh mục là bắt buộc")]
        public Guid CategoryId { get; set; }
        
        public bool IsVisible { get; set; } = true;
        
        [Range(0, 1000, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 1000")]
        public int DisplayOrder { get; set; }
        
        [StringLength(100, ErrorMessage = "Tên tùy chỉnh không được vượt quá 100 ký tự")]
        public string CustomName { get; set; }
        
        [StringLength(50, ErrorMessage = "Icon không được vượt quá 50 ký tự")]
        public string Icon { get; set; }
        
        public bool IsMainMenu { get; set; } = true;
        

        public Guid? ParentId { get; set; }
    }

    public class UpdateMenuConfigDto
    {
        [Required(ErrorMessage = "ID cấu hình menu là bắt buộc")]
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "ID danh mục là bắt buộc")]
        public Guid CategoryId { get; set; }
        
        public bool IsVisible { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Thứ tự hiển thị phải từ 0 đến 1000")]
        public int DisplayOrder { get; set; }
        
        [StringLength(100, ErrorMessage = "Tên tùy chỉnh không được vượt quá 100 ký tự")]
        public string CustomName { get; set; }
        
        [StringLength(50, ErrorMessage = "Icon không được vượt quá 50 ký tự")]
        public string Icon { get; set; }
        
        public bool IsMainMenu { get; set; }
        

        public Guid? ParentId { get; set; }
    }
} 
