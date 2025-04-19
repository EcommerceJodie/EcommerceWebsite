using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.DTOs.Identity
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Refresh token là bắt buộc")]
        public string RefreshToken { get; set; }
    }
} 
