using FluentAssertions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for Transaction endpoints.
    /// Tests HTTP requests end-to-end (routing, middleware, auth, database).
    /// </summary>
    public class TransactionEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the TransactionEndpointsTests class.
        /// </summary>
        /// <param name="factory">The custom WebApplicationFactory for integration tests.</param>
        public TransactionEndpointsTests(CustomWebApplicationFactory<Program> factory)
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

            // Seed category
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

            // Seed warehouse
            if (!db.Warehouses.Any())
            {
                db.Warehouses.Add(new Domain.Entities.Warehouse
                {
                    Id = 1,
                    Name = "Test Warehouse",
                    Location = "Test Location",
                    Capacity = 1000,
                    CurrentOccupancy = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            // Seed product
            if (!db.Products.Any())
            {
                db.Products.Add(new Domain.Entities.Product
                {
                    Id = 1,
                    Name = "Test Product",
                    SKU = "TEST-001",
                    Description = "Test Description",
                    Price = 100m,
                    StockQuantity = 100,
                    MinimumStockThreshold = 10,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            // Seed user (for CreatedByUserId FK)
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
        // Index (GET)
        // ============================================================

        [Fact]
        public async Task Transaction_Index_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Transaction");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (GET)
        // ============================================================

        [Fact]
        public async Task Transaction_Create_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Transaction/Create");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Transaction_Create_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var formData = new Dictionary<string, string>
            {
                ["Transaction.ProductId"] = "1",
                ["Transaction.WarehouseId"] = "1",
                ["Transaction.Type"] = "1",
                ["Transaction.Quantity"] = "10",
                ["UnitPriceRaw"] = "100"
            };

            var response = await _client.PostAsync("/Transaction/Create",
                new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Delete (POST)
        // ============================================================

        [Fact]
        public async Task Transaction_Delete_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.PostAsync("/Transaction/Delete/1",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }
    }
}
