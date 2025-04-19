using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Core.DTOs.Identity;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockTokenService = new Mock<ITokenService>();
            
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, 
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);
            
            _controller = new AuthController(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockTokenService.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkResult_WithToken()
        {

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = loginDto.Email,
                UserName = loginDto.Email,
                FirstName = "Test",
                LastName = "User"
            };

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = "test-jwt-token",
                Roles = new List<string> { "Admin" },
                CreatedAt = DateTime.UtcNow
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            
            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });
            
            _mockTokenService.Setup(x => x.CreateTokenAsync(user, new List<string> { "Admin" }))
                .ReturnsAsync((userDto.Token, "refresh-token"));


            var result = await _controller.Login(loginDto);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            
            var returnedUser = okResult.Value as UserDto;
            returnedUser.Should().NotBeNull();
            returnedUser.Email.Should().Be(user.Email);
            returnedUser.Token.Should().Be(userDto.Token);
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
        {

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((ApplicationUser)null);


            await Assert.ThrowsAsync<System.Security.Authentication.AuthenticationException>(() => 
                _controller.Login(loginDto));
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword!"
            };

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = loginDto.Email,
                UserName = loginDto.Email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            
            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);


            await Assert.ThrowsAsync<System.Security.Authentication.AuthenticationException>(() =>
                _controller.Login(loginDto));
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ShouldReturnOkResult_WithNewToken()
        {

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "old-refresh-token"
            };

            var userId = "user-id";
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Token = "new-jwt-token",
                RefreshToken = "new-refresh-token",
                Roles = new List<string> { "Admin" },
                CreatedAt = DateTime.UtcNow
            };

            _mockTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            
            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken))
                .ReturnsAsync(true);
            
            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });
            
            _mockTokenService.Setup(x => x.CreateTokenAsync(user, new List<string> { "Admin" }))
                .ReturnsAsync((userDto.Token, userDto.RefreshToken));


            var result = await _controller.RefreshToken(refreshTokenDto);


            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            
            var returnedUser = okResult.Value as UserDto;
            returnedUser.Should().NotBeNull();
            returnedUser.Token.Should().Be(userDto.Token);
            returnedUser.RefreshToken.Should().Be(userDto.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_WithInvalidToken_ShouldThrowAuthenticationException()
        {

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            _mockTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync((string)null);


            await Assert.ThrowsAsync<System.Security.Authentication.AuthenticationException>(() =>
                _controller.RefreshToken(refreshTokenDto));
        }

        [Fact]
        public async Task RefreshToken_WithExpiredToken_ShouldThrowAuthenticationException()
        {

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "expired-refresh-token"
            };

            var userId = "user-id";

            _mockTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            
            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken))
                .ReturnsAsync(false);


            await Assert.ThrowsAsync<System.Security.Authentication.AuthenticationException>(() =>
                _controller.RefreshToken(refreshTokenDto));
        }

        [Fact]
        public async Task RefreshToken_WithNonExistentUser_ShouldThrowKeyNotFoundException()
        {

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "valid-refresh-token"
            };

            var userId = "non-existent-user-id";

            _mockTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            
            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken))
                .ReturnsAsync(true);
            
            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);


            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.RefreshToken(refreshTokenDto));
        }

        [Fact]
        public async Task RefreshToken_WithUserLackingAdminRole_ShouldThrowUnauthorizedAccessException()
        {

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = "valid-refresh-token"
            };

            var userId = "user-id";
            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "test@example.com"
            };

            _mockTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(refreshTokenDto.RefreshToken))
                .ReturnsAsync(userId);
            
            _mockTokenService.Setup(x => x.ValidateRefreshTokenAsync(userId, refreshTokenDto.RefreshToken))
                .ReturnsAsync(true);
            
            _mockUserManager.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });


            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _controller.RefreshToken(refreshTokenDto));
        }
    }
}
