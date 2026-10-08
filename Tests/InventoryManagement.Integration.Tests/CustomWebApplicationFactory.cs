using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Integration.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
         
                var descriptorsToRemove = services
                    .Where(d =>
                        // DbContextOptions
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(DbContextOptions) ||

                        // DbContext
                        d.ServiceType == typeof(AppDbContext) ||

                        // IDbContextOptionsConfiguration
                        d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>) ||

                        // DbContextOptions<>
                        (d.ServiceType.IsGenericType &&
                         d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||

                        // Semua provider database
                        (d.ServiceType.FullName != null &&
                         (d.ServiceType.FullName.Contains("EntityFrameworkCore.SqlServer") ||
                          d.ServiceType.FullName.Contains("EntityFrameworkCore.InMemory") ||
                          d.ServiceType.FullName.Contains("SqlServerDatabaseProvider") ||
                          d.ServiceType.FullName.Contains("InMemoryDatabaseProvider"))) ||

                        // Implementation type terkait
                        (d.ImplementationType?.FullName != null &&
                         (d.ImplementationType.FullName.Contains("SqlServer") ||
                          d.ImplementationType.FullName.Contains("InMemory")))
                    )
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"IntegrationTestDb_{Guid.NewGuid()}");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
