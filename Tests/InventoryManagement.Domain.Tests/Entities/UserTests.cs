using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_Should_Have_Default_Values()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            user.Username.Should().BeEmpty();
            user.Email.Should().BeEmpty();
            user.PasswordHash.Should().BeEmpty();
            user.FullName.Should().BeEmpty();
            user.IsActive.Should().BeTrue(); // ← Default true
            user.LastLoginDate.Should().BeNull();
            user.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void User_Should_Allow_Setting_Properties()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "admin@test.com",
                PasswordHash = "hashed",
                FullName = "Admin User",
                Role = UserRole.Admin
            };

            // Assert
            user.Username.Should().Be("admin");
            user.Email.Should().Be("admin@test.com");
            user.FullName.Should().Be("Admin User");
            user.Role.Should().Be(UserRole.Admin);
        }

        [Fact]
        public void User_Should_Have_Empty_RefreshTokens_By_Default()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            user.RefreshTokens.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void User_Should_Have_Empty_InventoryTransactions_By_Default()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            user.InventoryTransactions.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void User_Admin_Role_Should_Be_Correct()
        {
            // Arrange & Act
            var user = new User { Role = UserRole.Admin };

            // Assert
            user.Role.Should().Be(UserRole.Admin);
            user.Role.Should().NotBe(UserRole.User);
        }

        [Fact]
        public void User_Regular_Role_Should_Be_Correct()
        {
            // Arrange & Act
            var user = new User { Role = UserRole.User };

            // Assert
            user.Role.Should().Be(UserRole.User);
            user.Role.Should().NotBe(UserRole.Admin);
        }
    }
}
