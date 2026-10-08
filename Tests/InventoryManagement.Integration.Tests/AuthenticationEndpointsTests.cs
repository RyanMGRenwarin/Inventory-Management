using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.RegularExpressions;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for authentication endpoints.
    /// Tests HTTP requests end-to-end (routing, middleware, auth, database).
    /// </summary>
    public class AuthenticationEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthenticationEndpointsTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        // ============================================================
        // Login Page
        // ============================================================

        [Fact]
        public async Task Login_Page_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/Auth/Auth/Login");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_Post_With_Invalid_Credentials_Should_Return_View()
        {
            // Arrange
            var getResponse = await _client.GetAsync("/Auth/Auth/Login");
            getResponse.EnsureSuccessStatusCode();

            var html = await getResponse.Content.ReadAsStringAsync();

            var csrfToken = ExtractAntiForgeryToken(html);

            var formData = new Dictionary<string, string>
            {
                ["Username"] = "invaliduser",
                ["Password"] = "wrongpassword",
                ["__RequestVerificationToken"] = csrfToken
            };

            // Act
            var response = await _client.PostAsync("/Auth/Auth/Login",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Invalid username or password");
        }

        // ============================================================
        // Register Page
        // ============================================================

        [Fact]
        public async Task Register_Page_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/Auth/Auth/Register");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ============================================================
        // Forgot Password Page
        // ============================================================

        [Fact]
        public async Task ForgotPassword_Page_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/Auth/Auth/ForgotPassword");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ============================================================
        // Logout
        // ============================================================

        [Fact]
        public async Task Logout_Get_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/Auth/Auth/Logout");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ============================================================
        // Authorization Checks
        // ============================================================

        [Fact]
        public async Task Home_Page_Should_Be_Accessible_Without_Auth()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Product_Index_Should_Redirect_To_Login_When_Not_Authenticated()
        {
            // Act
            var response = await _client.GetAsync("/Product");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Warehouse_Index_Should_Redirect_To_Login_When_Not_Authenticated()
        {
            // Act
            var response = await _client.GetAsync("/Warehouse");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Category_Index_Should_Redirect_To_Login_When_Not_Authenticated()
        {
            // Act
            var response = await _client.GetAsync("/Category");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Transaction_Index_Should_Redirect_To_Login_When_Not_Authenticated()
        {
            // Act
            var response = await _client.GetAsync("/Transaction");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Health Check
        // ============================================================

        [Fact]
        public async Task Health_Endpoint_Should_Return_Success()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Helper untuk extract CSRF token dari HTML.
        /// </summary>
        private static string ExtractAntiForgeryToken(string html)
        {
            // Search: <input name="__RequestVerificationToken" type="hidden" value="..." />
            var match = Regex.Match(
                html,
                @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""[^>]*>");

            if (!match.Success)
            {
                // Try another pattern (value before name)
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
