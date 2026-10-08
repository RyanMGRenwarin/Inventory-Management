using FluentAssertions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for Warehouse endpoints.
    /// Tests HTTP requests end-to-end (routing, middleware, auth, database).
    /// </summary>
    public class WarehouseEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the WarehouseEndpointsTests class.
        /// </summary>
        /// <param name="factory">The custom WebApplicationFactory for integration tests.</param>
        public WarehouseEndpointsTests(CustomWebApplicationFactory<Program> factory)
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

            db.SaveChanges();
        }

        // ============================================================
        // Index (GET)
        // ============================================================

        [Fact]
        public async Task Warehouse_Index_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Warehouse");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (GET)
        // ============================================================

        [Fact]
        public async Task Warehouse_Create_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Warehouse/Create");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Edit (GET)
        // ============================================================

        [Fact]
        public async Task Warehouse_Edit_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.GetAsync("/Warehouse/Edit/1");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Warehouse_Create_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var formData = new Dictionary<string, string>
            {
                ["Warehouse.Name"] = "New Warehouse",
                ["Warehouse.Location"] = "New Location",
                ["Warehouse.Capacity"] = "500"
            };

            var response = await _client.PostAsync("/Warehouse/Create",
                new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Edit (POST)
        // ============================================================

        [Fact]
        public async Task Warehouse_Edit_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var formData = new Dictionary<string, string>
            {
                ["WarehouseId"] = "1",
                ["Warehouse.Name"] = "Updated Warehouse",
                ["Warehouse.Location"] = "Updated Location",
                ["Warehouse.Capacity"] = "2000"
            };

            var response = await _client.PostAsync("/Warehouse/Edit/1",
                new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }

        // ============================================================
        // Delete (POST)
        // ============================================================

        [Fact]
        public async Task Warehouse_Delete_Post_Without_Auth_Should_Redirect_To_Login()
        {
            var response = await _client.PostAsync("/Warehouse/Delete/1",
                new FormUrlEncodedContent(new Dictionary<string, string>()));

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Redirect,
                HttpStatusCode.Unauthorized);
        }
    }
}
