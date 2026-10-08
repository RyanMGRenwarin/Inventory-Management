using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for health check endpoint.
    /// Verifies that the application exposes a working /health endpoint.
    /// </summary>
    public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the HealthCheckTests class.
        /// </summary>
        /// <param name="factory">The custom WebApplicationFactory for integration tests.</param>
        public HealthCheckTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        // ============================================================
        // Health Endpoint
        // ============================================================

        [Fact]
        public async Task Health_Endpoint_Should_Return_Success()
        {
            var response = await _client.GetAsync("/health");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Health_Endpoint_Should_Return_Healthy_Status()
        {
            var response = await _client.GetAsync("/health");

            var content = await response.Content.ReadAsStringAsync();

            // ASP.NET Core health check default response body is "Healthy"
            content.Should().Contain("Healthy");
        }

        [Fact]
        public async Task Health_Endpoint_Should_Not_Require_Authentication()
        {
            // Health check should be accessible without login (for load balancers, orchestrators)
            var response = await _client.GetAsync("/health");

            response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
            response.StatusCode.Should().NotBe(HttpStatusCode.Redirect);
        }
    }
}
