using FluentAssertions;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class RefreshTokenTests
    {
        [Fact]
        public void RefreshToken_Should_Have_Default_Values()
        {
            // Arrange & Act
            var token = new RefreshToken();

            // Assert
            token.UserId.Should().Be(0);
            token.Token.Should().BeEmpty();
            token.IsRevoked.Should().BeFalse();
            token.RevokedAt.Should().BeNull();
            token.CreatedByIp.Should().BeNull();
            token.RevokedByIp.Should().BeNull();
            token.ReplacedByToken.Should().BeNull();
            token.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void RefreshToken_Should_Allow_Setting_Properties()
        {
            // Arrange
            var expiry = DateTime.UtcNow.AddDays(7);
            var token = new RefreshToken
            {
                UserId = 1,
                Token = "token-value",
                ExpiryDate = expiry,
                CreatedByIp = "127.0.0.1"
            };

            // Assert
            token.UserId.Should().Be(1);
            token.Token.Should().Be("token-value");
            token.ExpiryDate.Should().Be(expiry);
            token.CreatedByIp.Should().Be("127.0.0.1");
        }

        [Fact]
        public void RefreshToken_Should_Not_Be_Revoked_By_Default()
        {
            // Arrange & Act
            var token = new RefreshToken
            {
                Token = "test-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            // Assert
            token.IsRevoked.Should().BeFalse();
            token.RevokedAt.Should().BeNull();
        }

        [Fact]
        public void RefreshToken_Can_Be_Revoked()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "test-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = "127.0.0.1";

            // Assert
            token.IsRevoked.Should().BeTrue();
            token.RevokedAt.Should().NotBeNull();
            token.RevokedByIp.Should().Be("127.0.0.1");
        }

        [Fact]
        public void RefreshToken_Can_Be_Rotated()
        {
            // Arrange
            var token = new RefreshToken
            {
                Token = "old-token",
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            // Act
            token.IsRevoked = true;
            token.ReplacedByToken = "new-token";

            // Assert
            token.IsRevoked.Should().BeTrue();
            token.ReplacedByToken.Should().Be("new-token");
        }

        [Fact]
        public void RefreshToken_ExpiryDate_Should_Be_In_Future()
        {
            // Arrange
            var expiry = DateTime.UtcNow.AddDays(7);
            var token = new RefreshToken
            {
                Token = "test-token",
                ExpiryDate = expiry
            };

            // Assert
            token.ExpiryDate.Should().BeAfter(DateTime.UtcNow);
        }
    }
}
