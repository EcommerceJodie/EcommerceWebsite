using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IRedisRepository _redisRepository;
        private const string RefreshTokenPrefix = "refresh_token:";
        private const string UserIdPrefix = "user_id:";

        public TokenService(IConfiguration configuration, IRedisRepository redisRepository)
        {
            _configuration = configuration;
            _redisRepository = redisRepository;
        }

        public async Task<(string accessToken, string refreshToken)> CreateTokenAsync(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
            {
                claims.Add(new Claim("FullName", user.FullName));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = await GenerateRefreshTokenAsync();


            await SaveRefreshTokenAsync(user.Id, refreshToken);

            return (accessToken, refreshToken);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken, int expiryInDays = 7)
        {

            var refreshTokenKey = $"{RefreshTokenPrefix}{userId}";
            var result1 = await _redisRepository.SetAsync(refreshTokenKey, refreshToken, TimeSpan.FromDays(expiryInDays));
            

            var userIdKey = $"{UserIdPrefix}{refreshToken}";
            var result2 = await _redisRepository.SetAsync(userIdKey, userId, TimeSpan.FromDays(expiryInDays));
            
            return result1 && result2;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken))
                return false;


            var key = $"{RefreshTokenPrefix}{userId}";
            var storedToken = await _redisRepository.GetAsync(key);
            

            return storedToken == refreshToken;
        }

        public async Task<bool> DeleteRefreshTokenAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
                

            var refreshTokenKey = $"{RefreshTokenPrefix}{userId}";
            var refreshToken = await _redisRepository.GetAsync(refreshTokenKey);
            
            if (string.IsNullOrEmpty(refreshToken))
                return true; 
                

            var userIdKey = $"{UserIdPrefix}{refreshToken}";
            var result1 = await _redisRepository.DeleteAsync(userIdKey);
            

            var result2 = await _redisRepository.DeleteAsync(refreshTokenKey);
            
            return result1 && result2;
        }

        public async Task<string> GetUserIdFromRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;
                

            var userIdKey = $"{UserIdPrefix}{refreshToken}";
            return await _redisRepository.GetAsync(userIdKey);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
            
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
} 
