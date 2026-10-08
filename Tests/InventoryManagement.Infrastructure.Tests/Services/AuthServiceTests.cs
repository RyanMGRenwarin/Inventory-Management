using FluentAssertions;
using InventoryManagement.Application.DTOs.Auth;
using InventoryManagement.Application.Helpers;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace InventoryManagement.Infrastructure.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly JwtSettings _jwtSettings;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userRepoMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _jwtSettings = new JwtSettings
            {
                SecretKey = "ThisIsASecretKeyForTestingPurposesOnly1234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7
            };

            _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepoMock.Object);

            _sut = new AuthService(
                _unitOfWorkMock.Object,
                Options.Create(_jwtSettings),
                _loggerMock.Object);
        }

        // ============================================================
        // LoginAsync
        // ============================================================

        [Fact]
        public async Task LoginAsync_Should_Return_Tokens_When_Credentials_Valid()
        {
            // Arrange
            var password = "Admin@123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@test.com",
                FullName = "Admin",
                PasswordHash = passwordHash,
                Role = UserRole.Admin,
                IsActive = true
            };

            var request = new LoginRequestDto { Username = "admin", Password = password };

            _userRepoMock.Setup(x => x.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.LoginAsync(request, "127.0.0.1");

            // Assert
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Role.Should().Be("Admin");
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Null_When_User_Not_Found()
        {
            // Arrange
            var request = new LoginRequestDto { Username = "unknown", Password = "test" };

            _userRepoMock.Setup(x => x.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _sut.LoginAsync(request, "127.0.0.1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Null_When_Password_Invalid()
        {
            // Arrange
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash,
                IsActive = true,
                Role = UserRole.Admin
            };

            var request = new LoginRequestDto { Username = "admin", Password = "WrongPassword" };

            _userRepoMock.Setup(x => x.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.LoginAsync(request, "127.0.0.1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_Should_Return_Null_When_User_Inactive()
        {
            // Arrange
            var password = "Admin@123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash,
                IsActive = false,
                Role = UserRole.Admin
            };

            var request = new LoginRequestDto { Username = "admin", Password = password };

            _userRepoMock.Setup(x => x.GetByUsernameAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.LoginAsync(request, "127.0.0.1");

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // RegisterAsync
        // ============================================================

        [Fact]
        public async Task RegisterAsync_Should_Create_User_When_All_Unique()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "new@test.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            _userRepoMock.Setup(x => x.UsernameExistsAsync("newuser", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.EmailExistsAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // Act
            var result = await _sut.RegisterAsync(request);

            // Assert
            result.Should().BeTrue();
            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Should_Return_False_When_Username_Exists()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "existing",
                Email = "new@test.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            _userRepoMock.Setup(x => x.UsernameExistsAsync("existing", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _sut.RegisterAsync(request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterAsync_Should_Return_False_When_Email_Exists()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "existing@test.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            _userRepoMock.Setup(x => x.UsernameExistsAsync("newuser", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _userRepoMock.Setup(x => x.EmailExistsAsync("existing@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            // Act
            var result = await _sut.RegisterAsync(request);

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // ChangePasswordAsync
        // ============================================================

        [Fact]
        public async Task ChangePasswordAsync_Should_Change_When_Current_Correct()
        {
            // Arrange
            var currentPassword = "OldPassword@123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword);

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash,
                IsActive = true
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.ChangePasswordAsync(1, currentPassword, "NewPassword@123");

            // Assert
            result.Should().BeTrue();
            _userRepoMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Return_False_When_Current_Wrong()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                IsActive = true
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.ChangePasswordAsync(1, "WrongPassword", "NewPassword@123");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            _userRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _sut.ChangePasswordAsync(999, "any", "NewPassword@123");

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // LogoutAsync
        // ============================================================

        [Fact]
        public async Task LogoutAsync_Should_Revoke_Token()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "admin",
                RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Id = 1,
                    Token = "test-token",
                    UserId = 1,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                }
            }
            };

            _userRepoMock.Setup(x => x.GetUserByRefreshTokenAsync("test-token", It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.LogoutAsync("test-token", "127.0.0.1");

            // Assert
            result.Should().BeTrue();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            _userRepoMock.Setup(x => x.GetUserByRefreshTokenAsync("unknown", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _sut.LogoutAsync("unknown", "127.0.0.1");

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // RevokeAllTokensAsync
        // ============================================================

        [Fact]
        public async Task RevokeAllTokensAsync_Should_Revoke_All()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "admin",
                RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken { Id = 1, Token = "token1", UserId = 1, ExpiryDate = DateTime.UtcNow.AddDays(7), IsRevoked = false },
                new RefreshToken { Id = 2, Token = "token2", UserId = 1, ExpiryDate = DateTime.UtcNow.AddDays(7), IsRevoked = false }
            }
            };

            _userRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await _sut.RevokeAllTokensAsync(1);

            // Assert
            result.Should().BeTrue();
            user.RefreshTokens.Should().AllSatisfy(rt => rt.IsRevoked.Should().BeTrue());
        }

        [Fact]
        public async Task RevokeAllTokensAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            _userRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

            // Act
            var result = await _sut.RevokeAllTokensAsync(999);

            // Assert
            result.Should().BeFalse();
        }
    }
}
