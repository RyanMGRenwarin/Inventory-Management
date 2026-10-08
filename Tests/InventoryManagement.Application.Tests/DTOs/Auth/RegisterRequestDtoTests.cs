using FluentAssertions;
using InventoryManagement.Application.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Auth
{
    public class RegisterRequestDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void RegisterRequestDto_Should_Be_Valid_When_All_Correct()
        {
            var dto = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void RegisterRequestDto_Should_Fail_When_Email_Invalid()
        {
            var dto = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "not-an-email",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterRequestDto_Should_Fail_When_Password_Mismatch()
        {
            var dto = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Different@123"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void RegisterRequestDto_Should_Fail_When_Username_Too_Short()
        {
            var dto = new RegisterRequestDto
            {
                Username = "ab",
                Email = "newuser@example.com",
                FullName = "New User",
                Password = "Password@123",
                ConfirmPassword = "Password@123"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void RegisterRequestDto_Should_Fail_When_Password_Too_Short()
        {
            var dto = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                FullName = "New User",
                Password = "12345",
                ConfirmPassword = "12345"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }
    }
}
