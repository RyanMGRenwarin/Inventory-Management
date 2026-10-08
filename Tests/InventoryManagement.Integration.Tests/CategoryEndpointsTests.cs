using FluentAssertions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for Category endpoints.
    /// Tests HTTP requests end-to-end (routing, middleware, auth, database).
    /// </summary>
    public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the CategoryEndpointsTests class.
        /// </summary>
        /// <param name="factory">The custom WebApplicationFactory for integration tests.</param>
        public CategoryEndpointsTests(CustomWebApplicationFactory<Program> factory)
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

            if (!db.Categories.Any())
            {
                db.Categories.Add(new Domain.Entities.Category
                {
                    Id = 1,
                    Name = "Test Category",
                    Description = "Test Description",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            db.SaveChanges();
        }

        // ============================================================
        // Index (GET)
        // ============================================================

        [Fact]
        public async Task Category_Index_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Category");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (GET)
        // ============================================================

        [Fact]
        public async Task Category_Create_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Category/Create");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Edit (GET)
        // ============================================================

        [Fact]
        public async Task Category_Edit_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Category/Edit/1");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Category_Create_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var formData = new Dictionary<string, string>
            {
                ["Category.Name"] = "New Category",
                ["Category.Description"] = "New Description"
            };

            var response = await _client.PostAsync("/Category/Create",
                new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Edit (POST)
        // ============================================================

        [Fact]
        public async Task Category_Edit_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var formData = new Dictionary<string, string>
            {
                ["CategoryId"] = "1",
                ["Category.Name"] = "Updated Category",
                ["Category.Description"] = "Updated Description"
            };

            var response = await _client.PostAsync("/Category/Edit/1",
                new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Delete (POST)
        // ============================================================

        [Fact]
        public async Task Category_Delete_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.PostAsync("/Category/Delete/1",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }
    }
}
