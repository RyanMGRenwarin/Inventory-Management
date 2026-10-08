using FluentAssertions;
using InventoryManagement.Application.DTOs.Auth;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Auth
{
    public class LoginRequestDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void LoginRequestDto_Should_Be_Valid_When_All_Correct()
        {
            var dto = new LoginRequestDto
            {
                Username = "admin",
                Password = "Admin@123",
                RememberMe = false
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void LoginRequestDto_Should_Fail_When_Username_Empty()
        {
            var dto = new LoginRequestDto
            {
                Username = "",
                Password = "Admin@123"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void LoginRequestDto_Should_Fail_When_Username_Too_Short()
        {
            var dto = new LoginRequestDto
            {
                Username = "ab",
                Password = "Admin@123"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void LoginRequestDto_Should_Fail_When_Password_Empty()
        {
            var dto = new LoginRequestDto
            {
                Username = "admin",
                Password = ""
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginRequestDto_Should_Fail_When_Password_Too_Short()
        {
            var dto = new LoginRequestDto
            {
                Username = "admin",
                Password = "12345"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Password"));
        }
    }
}
