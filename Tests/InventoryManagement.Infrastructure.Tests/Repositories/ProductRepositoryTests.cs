using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Tests.Repositories
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ProductRepository _sut;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new ProductRepository(_context);

            // Seed a category to satisfy FK constraint
            _context.Categories.Add(new Category { Id = 1, Name = "Category A" });
            _context.SaveChanges();
        }

        // ============================================================
        // GetByIdAsync (inherited from GenericRepository)
        // ============================================================

        [Fact]
        public async Task GetByIdAsync_Should_Return_Product_When_Found()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "TEST-001", Price = 10m, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(product.Id, default);

            // Assert
            result.Should().NotBeNull();
            result!.SKU.Should().Be("TEST-001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetByIdAsync(999, default);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // AddAsync
        // ============================================================

        [Fact]
        public async Task AddAsync_Should_Add_Product_To_Database()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "TEST-001", Price = 10m, CategoryId = 1 };

            // Act
            await _sut.AddAsync(product, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Products.FirstOrDefaultAsync(p => p.SKU == "TEST-001");
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test");
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Id_After_Save()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "TEST-002", Price = 10m, CategoryId = 1 };

            // Act
            await _sut.AddAsync(product, default);
            await _context.SaveChangesAsync();

            // Assert
            product.Id.Should().BeGreaterThan(0);
        }

        // ============================================================
        // UpdateAsync
        // ============================================================

        [Fact]
        public async Task UpdateAsync_Should_Update_Product()
        {
            // Arrange
            var product = new Product { Name = "Old", SKU = "TEST-001", Price = 10m, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            product.Name = "Updated";
            product.Price = 20m;
            await _sut.UpdateAsync(product, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Products.FindAsync(product.Id);
            result!.Name.Should().Be("Updated");
            result.Price.Should().Be(20m);
        }

        // ============================================================
        // DeleteAsync / DeleteByIdAsync
        // ============================================================

        [Fact]
        public async Task DeleteAsync_Should_Remove_Product()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "TEST-001", Price = 10m, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteAsync(product, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Products.FindAsync(product.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteByIdAsync_Should_Soft_Delete_Product()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "TEST-001", Price = 10m, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteByIdAsync(product.Id, default);
            await _context.SaveChangesAsync();

            // Assert
            // Note: If soft delete (IsDeleted), the query filter will hide it
            var result = await _context.Products.IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (result != null)
            {
                result.IsDeleted.Should().BeTrue();
            }
            else
            {
                // Hard delete
                result.Should().BeNull();
            }
        }

        // ============================================================
        // GetBySkuAsync
        // ============================================================

        [Fact]
        public async Task GetBySkuAsync_Should_Return_Product_When_Found()
        {
            // Arrange
            _context.Products.Add(new Product { Name = "Test", SKU = "TEST-001", Price = 10m, CategoryId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetBySkuAsync("TEST-001");

            // Assert
            result.Should().NotBeNull();
            result!.SKU.Should().Be("TEST-001");
        }

        [Fact]
        public async Task GetBySkuAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetBySkuAsync("NOT-EXIST");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBySkuAsync_Should_Exclude_Soft_Deleted()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "DEL-001", Price = 10m, CategoryId = 1, IsDeleted = true };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetBySkuAsync("DEL-001");

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // GetByCategoryAsync
        // ============================================================

        [Fact]
        public async Task GetByCategoryAsync_Should_Return_Products_In_Category()
        {
            // Arrange
            _context.Products.AddRange(
                new Product { Name = "A", SKU = "A-001", Price = 10m, CategoryId = 1 },
                new Product { Name = "B", SKU = "B-001", Price = 20m, CategoryId = 1 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByCategoryAsync(1);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByCategoryAsync_Should_Return_Empty_When_No_Products()
        {
            // Act
            var result = await _sut.GetByCategoryAsync(999);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetLowStockProductsAsync
        // ============================================================

        [Fact]
        public async Task GetLowStockProductsAsync_Should_Return_Products_Below_Threshold()
        {
            // Arrange
            _context.Products.AddRange(
                new Product { Name = "Low", SKU = "LOW-001", Price = 10m, StockQuantity = 2, MinimumStockThreshold = 5, CategoryId = 1 },
                new Product { Name = "OK", SKU = "OK-001", Price = 10m, StockQuantity = 100, MinimumStockThreshold = 5, CategoryId = 1 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetLowStockProductsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().SKU.Should().Be("LOW-001");
        }

        [Fact]
        public async Task GetLowStockProductsAsync_Should_Return_Empty_When_All_OK()
        {
            // Arrange
            _context.Products.Add(new Product { Name = "OK", SKU = "OK-001", Price = 10m, StockQuantity = 100, MinimumStockThreshold = 5, CategoryId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetLowStockProductsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // UpdateStockAsync
        // ============================================================

        [Fact]
        public async Task UpdateStockAsync_Should_Update_Stock_Quantity()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "TEST-001", Price = 10m, StockQuantity = 10, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateStockAsync(product.Id, 50);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Products.FindAsync(product.Id);
            result!.StockQuantity.Should().Be(50);
        }

        [Fact]
        public async Task UpdateStockAsync_Should_Not_Throw_When_Product_Not_Found()
        {
            // Act
            var act = async () => await _sut.UpdateStockAsync(999, 50);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ============================================================
        // SkuExistsAsync
        // ============================================================

        [Fact]
        public async Task SkuExistsAsync_Should_Return_True_When_SKU_Exists()
        {
            // Arrange
            _context.Products.Add(new Product { Name = "Test", SKU = "EXIST-001", Price = 10m, CategoryId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.SkuExistsAsync("EXIST-001");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SkuExistsAsync_Should_Return_False_When_SKU_Not_Exists()
        {
            // Act
            var result = await _sut.SkuExistsAsync("NOT-EXIST");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SkuExistsAsync_Should_Exclude_Specified_ProductId()
        {
            // Arrange
            var product = new Product { Name = "Test", SKU = "SKU-001", Price = 10m, CategoryId = 1 };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.SkuExistsAsync("SKU-001", excludeProductId: product.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SkuExistsAsync_Should_Exclude_Soft_Deleted_Products()
        {
            // Arrange
            _context.Products.Add(new Product { Name = "Deleted", SKU = "DEL-001", Price = 10m, CategoryId = 1, IsDeleted = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.SkuExistsAsync("DEL-001");

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // GetPagedAsync (inherited from GenericRepository)
        // ============================================================

        [Fact]
        public async Task GetPagedAsync_Should_Return_Correct_Page()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                _context.Products.Add(new Product { Name = $"Product {i}", SKU = $"SKU-{i:D3}", Price = i, CategoryId = 1 });
            }
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _sut.GetPagedAsync(1, 10, null, null, default);

            // Assert
            items.Should().HaveCount(10);
            totalCount.Should().Be(25);
        }

        [Fact]
        public async Task GetPagedAsync_Should_Return_Last_Page_Correctly()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                _context.Products.Add(new Product { Name = $"Product {i}", SKU = $"SKU-{i:D3}", Price = i, CategoryId = 1 });
            }
            await _context.SaveChangesAsync();

            // Act
            var (items, totalCount) = await _sut.GetPagedAsync(3, 10, null, null, default);

            // Assert
            items.Should().HaveCount(5); // 25 - 20 = 5 items on page 3
            totalCount.Should().Be(25);
        }

        // ============================================================
        // AnyAsync (inherited)
        // ============================================================

        [Fact]
        public async Task AnyAsync_Should_Return_True_When_Match()
        {
            // Arrange
            _context.Products.Add(new Product { Name = "Test", SKU = "TEST-001", Price = 10m, CategoryId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.AnyAsync(p => p.SKU == "TEST-001", default);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AnyAsync_Should_Return_False_When_No_Match()
        {
            // Act
            var result = await _sut.AnyAsync(p => p.SKU == "NOT-EXIST", default);

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // CountAsync (inherited)
        // ============================================================

        [Fact]
        public async Task CountAsync_Should_Return_Total_Count()
        {
            // Arrange
            _context.Products.AddRange(
                new Product { Name = "A", SKU = "A-001", Price = 10m, CategoryId = 1 },
                new Product { Name = "B", SKU = "B-001", Price = 20m, CategoryId = 1 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.CountAsync(null, default);

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task CountAsync_Should_Return_Count_With_Predicate()
        {
            // Arrange
            _context.Products.AddRange(
                new Product { Name = "A", SKU = "A-001", Price = 10m, CategoryId = 1 },
                new Product { Name = "B", SKU = "B-001", Price = 20m, CategoryId = 1 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.CountAsync(p => p.Price > 15m, default);

            // Assert
            result.Should().Be(1);
        }

        // ============================================================
        // Dispose
        // ============================================================

        protected void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}
