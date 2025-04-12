using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Core.DTOs.Identity;
using Ecommerce.Core.Models.Entities;
using Ecommerce.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Ecommerce.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

            _tokenServiceMock = new Mock<ITokenService>();

            _controller = new AuthController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object);

            // Thiết lập ClaimsPrincipal cho controller
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Login_ShouldReturnUserDto_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = "userId",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User"
            };

            var roles = new List<string> { "Admin" };
            var token = "jwt-token";

            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _tokenServiceMock.Setup(ts => ts.CreateTokenAsync(user, roles))
                .ReturnsAsync(token);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedUserDto = okResult.Value as UserDto;
            returnedUserDto.Should().NotBeNull();
            returnedUserDto.Email.Should().Be(user.Email);
            returnedUserDto.Token.Should().Be(token);
            returnedUserDto.Roles.Should().BeEquivalentTo(roles);
        }

        [Fact]
        public async Task Login_ShouldThrowAuthenticationException_WhenEmailDoesNotExist()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            await Assert.ThrowsAsync<AuthenticationException>(() => _controller.Login(loginDto));
        }

        [Fact]
        public async Task Login_ShouldThrowAuthenticationException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "admin@example.com",
                Password = "WrongPassword"
            };

            var user = new ApplicationUser
            {
                Id = "userId",
                Email = "admin@example.com"
            };

            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act & Assert
            await Assert.ThrowsAsync<AuthenticationException>(() => _controller.Login(loginDto));
        }

        [Fact]
        public async Task Login_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAdmin()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "user@example.com",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                Id = "userId",
                Email = "user@example.com"
            };

            var roles = new List<string> { "User" };

            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Login(loginDto));
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnUserDto_WhenUserIsLoggedIn()
        {
            // Arrange
            var email = "admin@example.com";
            var user = new ApplicationUser
            {
                Id = "userId",
                Email = email,
                FirstName = "Admin",
                LastName = "User"
            };

            var roles = new List<string> { "Admin" };
            var token = "jwt-token";

            _userManagerMock.Setup(um => um.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _tokenServiceMock.Setup(ts => ts.CreateTokenAsync(user, roles))
                .ReturnsAsync(token);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();

            var returnedUserDto = okResult.Value as UserDto;
            returnedUserDto.Should().NotBeNull();
            returnedUserDto.Email.Should().Be(email);
            returnedUserDto.Token.Should().Be(token);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetCurrentUser());
        }

        [Fact]
        public async Task Logout_ShouldReturnOkResult()
        {
            // Arrange
            _signInManagerMock.Setup(sm => sm.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            
            dynamic response = okResult.Value;
            string message = response.GetType().GetProperty("message").GetValue(response, null);
            Assert.Equal("Đăng xuất thành công", message);
        }
    }
} 