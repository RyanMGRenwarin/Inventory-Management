using FluentAssertions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for Product endpoints.
    /// Tests HTTP requests end-to-end (routing, auth, model binding, database).
    /// </summary>
    public class ProductEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProductEndpointsTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Setup test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure database created
            db.Database.EnsureCreated();

            // Seed category if empty
            if (!db.Categories.Any())
            {
                db.Categories.Add(new Domain.Entities.Category
                {
                    Id = 1,
                    Name = "Test Category",
                    Description = "Test Category Description",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            // Seed product if empty
            if (!db.Products.Any())
            {
                db.Products.Add(new Domain.Entities.Product
                {
                    Id = 1,
                    Name = "Test Product",
                    SKU = "TEST-001",
                    Description = "Test Description",
                    Price = 99.99m,
                    StockQuantity = 100,
                    MinimumStockThreshold = 10,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            db.SaveChanges();
        }

        // ============================================================
        // Index (GET) - Authorization
        // ============================================================

        [Fact]
        public async Task Product_Index_Without_Auth_Should_Redirect_To_Login()
        {
            // Act
            var response = await _client.GetAsync("/Product");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (GET) - Authorization
        // ============================================================

        [Fact]
        public async Task Product_Create_Without_Auth_Should_Redirect_To_Login()
        {
            // Act
            var response = await _client.GetAsync("/Product/Create");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Edit (GET) - Authorization
        // ============================================================

        [Fact]
        public async Task Product_Edit_Without_Auth_Should_Redirect_To_Login()
        {
            // Act
            var response = await _client.GetAsync("/Product/Edit/1");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Delete (GET) - Authorization
        // ============================================================

        [Fact]
        public async Task Product_Delete_Without_Auth_Should_Redirect_To_Login()
        {
            // Act
            var response = await _client.GetAsync("/Product/Delete/1");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // CheckSku (AJAX) - Authorization
        // ============================================================

        [Fact]
        public async Task Product_CheckSku_Without_Auth_Should_Redirect_To_Login()
        {
            // Act
            var response = await _client.GetAsync("/Product/CheckSku?sku=TEST-001");

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // POST Endpoints - Authorization
        // ============================================================

        [Fact]
        public async Task Product_Create_Post_Without_Auth_Should_Redirect_To_Login()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                ["Product.Name"] = "Test",
                ["Product.SKU"] = "TEST-002",
                ["PriceRaw"] = "100"
            };

            // Act
            var response = await _client.PostAsync("/Product/Create",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_Edit_Post_Without_Auth_Should_Redirect_To_Login()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                ["Product.Id"] = "1",
                ["Product.Name"] = "Updated",
                ["PriceRaw"] = "150"
            };

            // Act
            var response = await _client.PostAsync("/Product/Edit/1",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Product_Delete_Post_Without_Auth_Should_Redirect_To_Login()
        {
            // Act
            var response = await _client.PostAsync("/Product/Delete/1",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }
    }
}
