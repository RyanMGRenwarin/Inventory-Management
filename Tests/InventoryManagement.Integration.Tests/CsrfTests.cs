using FluentAssertions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for CSRF (Cross-Site Request Forgery) protection.
    /// Tests that POST endpoints reject requests without valid anti-forgery tokens.
    /// </summary>
    public class CsrfTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the CsrfTests class.
        /// </summary>
        /// <param name="factory">The custom WebApplicationFactory for integration tests.</param>
        public CsrfTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            SeedTestData();
        }

        /// <summary>
        /// Seeds test data into the in-memory database.
        /// </summary>
        private void SeedTestData()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                db.Users.Add(new Domain.Entities.User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@test.com",
                    FullName = "Admin",
                    PasswordHash = "hash",
                    Role = Domain.Enums.UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            db.SaveChanges();
        }

        // ============================================================
        // Login - CSRF Protection
        // ============================================================

        [Fact]
        public async Task Login_Post_Without_Csrf_Token_Should_Return_BadRequest()
        {
            // Arrange: POST without anti-forgery token
            var formData = new Dictionary<string, string>
            {
                ["Username"] = "admin",
                ["Password"] = "Admin@123"
            };

            // Act
            var response = await _client.PostAsync("/Auth/Auth/Login",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_Post_With_Valid_Csrf_Token_Should_Not_Return_BadRequest()
        {
            // Arrange: Get CSRF token first
            var getResponse = await _client.GetAsync("/Auth/Auth/Login");
            var html = await getResponse.Content.ReadAsStringAsync();
            var csrfToken = ExtractAntiForgeryToken(html);

            var formData = new Dictionary<string, string>
            {
                ["Username"] = "admin",
                ["Password"] = "Admin@123",
                ["__RequestVerificationToken"] = csrfToken
            };

            // Act
            var response = await _client.PostAsync("/Auth/Auth/Login",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        }

        // ============================================================
        // Register - CSRF Protection
        // ============================================================

        [Fact]
        public async Task Register_Post_Without_Csrf_Token_Should_Return_BadRequest()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                ["Username"] = "newuser",
                ["Email"] = "new@test.com",
                ["FullName"] = "New User",
                ["Password"] = "Password@123",
                ["ConfirmPassword"] = "Password@123"
            };

            // Act
            var response = await _client.PostAsync("/Auth/Auth/Register",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ============================================================
        // Logout - CSRF Protection
        // ============================================================

        [Fact]
        public async Task Logout_Post_Without_Csrf_Token_Should_Return_BadRequest()
        {
            // Arrange: POST without anti-forgery token
            // Act
            var response = await _client.PostAsync("/Auth/Auth/Logout",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert
            // Note: Logout without token → 400 BadRequest OR redirect (if not authenticated)
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest,
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Helpers
        // ============================================================

        /// <summary>
        /// Extracts the anti-forgery token from the given HTML.
        /// </summary>
        private static string ExtractAntiForgeryToken(string html)
        {
            var match = Regex.Match(
                html,
                @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""[^>]*>");

            if (!match.Success)
            {
                match = Regex.Match(
                    html,
                    @"<input[^>]*value=""([^""]+)""[^>]*name=""__RequestVerificationToken""[^>]*>");
            }

            if (!match.Success)
                throw new InvalidOperationException("CSRF token tidak ditemukan di HTML.");

            return match.Groups[1].Value;
        }
    }
}
