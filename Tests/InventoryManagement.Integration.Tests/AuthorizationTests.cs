using FluentAssertions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace InventoryManagement.Integration.Tests
{
    /// <summary>
    /// Integration tests for role-based authorization.
    /// Tests Admin vs User access on protected endpoints.
    /// </summary>
    public class AuthorizationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;

        /// <summary>
        /// Initializes a new instance of the AuthorizationTests class.
        /// </summary>
        /// <param name="factory">The custom WebApplicationFactory for integration tests.</param>
        public AuthorizationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
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
                    Description = "Test",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            if (!db.Warehouses.Any())
            {
                db.Warehouses.Add(new Domain.Entities.Warehouse
                {
                    Id = 1,
                    Name = "Test Warehouse",
                    Location = "Test",
                    Capacity = 1000,
                    CurrentOccupancy = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            if (!db.Products.Any())
            {
                db.Products.Add(new Domain.Entities.Product
                {
                    Id = 1,
                    Name = "Test Product",
                    SKU = "TEST-001",
                    Description = "Test",
                    Price = 100m,
                    StockQuantity = 100,
                    MinimumStockThreshold = 10,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Creates an HTTP client authenticated as the specified role.
        /// </summary>
        /// <param name="role">The role to authenticate as (Admin or User).</param>
        /// <returns>An authenticated HttpClient.</returns>
        private HttpClient CreateClientWithRole(string role)
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Override authentication to use TestAuthHandler
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, options => { });

                    // Store role for TestAuthHandler via DI
                    services.AddSingleton(new TestAuthOptions { Role = role });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        // ============================================================
        // Product - Admin Only Actions
        // ============================================================

        [Fact]
        public async Task Admin_Can_Access_Product_Create()
        {
            var client = CreateClientWithRole("Admin");

            var response = await client.GetAsync("/Product/Create");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task User_Cannot_Access_Product_Create()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Product/Create");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Admin_Can_Access_Product_Edit()
        {
            var client = CreateClientWithRole("Admin");

            var response = await client.GetAsync("/Product/Edit/1");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Found);
        }

        [Fact]
        public async Task User_Cannot_Access_Product_Edit()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Product/Edit/1");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Admin_Can_Access_Product_Delete()
        {
            var client = CreateClientWithRole("Admin");

            var response = await client.GetAsync("/Product/Delete/1");

            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Found);
        }

        [Fact]
        public async Task User_Cannot_Access_Product_Delete()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Product/Delete/1");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ============================================================
        // Category - Admin Only Actions
        // ============================================================

        [Fact]
        public async Task Admin_Can_Access_Category_Create()
        {
            var client = CreateClientWithRole("Admin");

            var response = await client.GetAsync("/Category/Create");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task User_Cannot_Access_Category_Create()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Category/Create");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ============================================================
        // Warehouse - Admin Only Actions
        // ============================================================

        [Fact]
        public async Task Admin_Can_Access_Warehouse_Create()
        {
            var client = CreateClientWithRole("Admin");

            var response = await client.GetAsync("/Warehouse/Create");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task User_Cannot_Access_Warehouse_Create()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Warehouse/Create");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ============================================================
        // Shared Access - Both Roles
        // ============================================================

        [Fact]
        public async Task Admin_Can_Access_Product_Index()
        {
            var client = CreateClientWithRole("Admin");

            var response = await client.GetAsync("/Product");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task User_Can_Access_Product_Index()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Product");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task User_Can_Access_Transaction_Create()
        {
            var client = CreateClientWithRole("User");

            var response = await client.GetAsync("/Transaction/Create");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Options for the TestAuthHandler to specify role during tests.
    /// </summary>
    public class TestAuthOptions
    {
        public string Role { get; set; } = "User";
    }

    /// <summary>
    /// Test authentication handler that creates a ClaimsPrincipal with the specified role.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "TestScheme";
        private readonly TestAuthOptions _options;

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            TestAuthOptions testOptions)
            : base(options, logger, encoder)
        {
            _options = testOptions;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.Role, _options.Role)
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
