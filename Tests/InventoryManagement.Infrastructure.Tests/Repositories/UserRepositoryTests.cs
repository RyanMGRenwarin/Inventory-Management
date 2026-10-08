using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserRepository _sut;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new UserRepository(_context);
        }

        // ============================================================
        // GetByIdAsync (inherited)
        // ============================================================

        [Fact]
        public async Task GetByIdAsync_Should_Return_User_When_Found()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(user.Id, default);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("admin");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetByIdAsync(999, default);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // GetByUsernameAsync
        // ============================================================

        [Fact]
        public async Task GetByUsernameAsync_Should_Return_User()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByUsernameAsync("admin");

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("admin");
        }

        [Fact]
        public async Task GetByUsernameAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetByUsernameAsync("notexist");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUsernameAsync_Should_Exclude_Soft_Deleted()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "deleted",
                Email = "d@d.com",
                FullName = "D",
                PasswordHash = "hash",
                Role = UserRole.User,
                IsActive = true,
                IsDeleted = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByUsernameAsync("deleted");

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // GetByEmailAsync
        // ============================================================

        [Fact]
        public async Task GetByEmailAsync_Should_Return_User()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@test.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByEmailAsync("admin@test.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("admin@test.com");
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetByEmailAsync("notexist@test.com");

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // GetUserWithRefreshTokensAsync
        // ============================================================

        [Fact]
        public async Task GetUserWithRefreshTokensAsync_Should_Include_Tokens()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = "token-1",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserWithRefreshTokensAsync("admin");

            // Assert
            result.Should().NotBeNull();
            result!.RefreshTokens.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUserWithRefreshTokensAsync_Should_Exclude_Revoked()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.RefreshTokens.AddRange(
                new RefreshToken { UserId = user.Id, Token = "active", ExpiryDate = DateTime.UtcNow.AddDays(7), IsRevoked = false },
                new RefreshToken { UserId = user.Id, Token = "revoked", ExpiryDate = DateTime.UtcNow.AddDays(7), IsRevoked = true }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserWithRefreshTokensAsync("admin");

            // Assert
            result!.RefreshTokens.Should().HaveCount(1);
        }

        // ============================================================
        // GetUserByRefreshTokenAsync
        // ============================================================

        [Fact]
        public async Task GetUserByRefreshTokenAsync_Should_Return_User()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = "valid-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserByRefreshTokenAsync("valid-token");

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("admin");
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_Should_Return_Null_When_Revoked()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = "revoked-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUserByRefreshTokenAsync("revoked-token");

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // UsernameExistsAsync
        // ============================================================

        [Fact]
        public async Task UsernameExistsAsync_Should_Return_True_When_Exists()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "a@a.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.UsernameExistsAsync("admin");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UsernameExistsAsync_Should_Return_False_When_Not_Exists()
        {
            // Act
            var result = await _sut.UsernameExistsAsync("notexist");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UsernameExistsAsync_Should_Exclude_Soft_Deleted()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "deleted",
                Email = "d@d.com",
                FullName = "D",
                PasswordHash = "hash",
                Role = UserRole.User,
                IsActive = true,
                IsDeleted = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.UsernameExistsAsync("deleted");

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // EmailExistsAsync
        // ============================================================

        [Fact]
        public async Task EmailExistsAsync_Should_Return_True_When_Exists()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@test.com",
                FullName = "Admin",
                PasswordHash = "hash",
                Role = UserRole.Admin,
                IsActive = true
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.EmailExistsAsync("admin@test.com");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task EmailExistsAsync_Should_Return_False_When_Not_Exists()
        {
            // Act
            var result = await _sut.EmailExistsAsync("notexist@test.com");

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // Dispose
        // ============================================================

        protected void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}
