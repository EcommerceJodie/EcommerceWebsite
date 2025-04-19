using Ecommerce.Core.Models.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Core.Interfaces.Services
{
    public interface ITokenService
    {
        Task<(string accessToken, string refreshToken)> CreateTokenAsync(ApplicationUser user, IList<string> roles);
        ClaimsPrincipal ValidateToken(string token);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken, int expiryInDays = 7);
        Task<bool> DeleteRefreshTokenAsync(string userId);
        Task<string> GetUserIdFromRefreshTokenAsync(string refreshToken);
    }
} 
