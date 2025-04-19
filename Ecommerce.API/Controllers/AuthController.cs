using Ecommerce.Core.DTOs.Identity;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new AuthenticationException("Email không tồn tại");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                throw new AuthenticationException("Mật khẩu không đúng");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("Tài khoản không có quyền truy cập trang quản trị");
            }

            var userDto = await CreateUserDtoWithRefreshToken(user);
            return Ok(userDto);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var userId = await _tokenService.GetUserIdFromRefreshTokenAsync(refreshTokenDto.RefreshToken);
            if (string.IsNullOrEmpty(userId))
            {
                throw new AuthenticationException("Refresh token không hợp lệ");
            }


            var isValid = await _tokenService.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken);
            if (!isValid)
            {
                throw new AuthenticationException("Refresh token không hợp lệ hoặc đã hết hạn");
            }


            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin người dùng");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
            {
                throw new UnauthorizedAccessException("Tài khoản không có quyền truy cập trang quản trị");
            }


            var userDto = await CreateUserDtoWithRefreshToken(user);
            return Ok(userDto);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng đăng nhập");
            }
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin người dùng");
            }

            var userDto = await CreateUserDto(user);
            return Ok(userDto);
        }

        [HttpPost("logout")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {

                await _tokenService.DeleteRefreshTokenAsync(userId);
            }

            await _signInManager.SignOutAsync();
            return Ok(new { message = "Đăng xuất thành công" });
        }

        private async Task<UserDto> CreateUserDto(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tokenResult = await _tokenService.CreateTokenAsync(user, roles);
            string token = tokenResult.accessToken;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Token = token,
                Roles = roles,
                CreatedAt = user.CreatedAt
            };
        }

        private async Task<UserDto> CreateUserDtoWithRefreshToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tokenResult = await _tokenService.CreateTokenAsync(user, roles);
            string accessToken = tokenResult.accessToken;
            string refreshToken = tokenResult.refreshToken;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Token = accessToken,
                RefreshToken = refreshToken,
                Roles = roles,
                CreatedAt = user.CreatedAt
            };
        }
    }
} 
