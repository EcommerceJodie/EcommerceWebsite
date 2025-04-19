using Ecommerce.Core.Models.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Services.Interfaces
{
    public interface ITokenService
    {
        Task<(string accessToken, string refreshToken)> CreateTokenAsync(ApplicationUser user, IList<string> roles);
        ClaimsPrincipal ValidateToken(string token);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(ApplicationUser user, string refreshToken);
    }
} 
