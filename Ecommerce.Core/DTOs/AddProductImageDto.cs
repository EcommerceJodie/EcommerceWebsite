using Microsoft.AspNetCore.Http;

namespace Ecommerce.Core.DTOs
{
    public class AddProductImageDto
    {
        public IFormFile Image { get; set; }
        public string ImageAltText { get; set; }
        public bool IsMainImage { get; set; } = false;
    }
} 
