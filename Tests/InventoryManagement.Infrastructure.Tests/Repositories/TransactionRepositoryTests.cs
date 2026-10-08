using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Tests.Repositories
{
    public class TransactionRepositoryTests
    {
        private readonly AppDbContext _context;
        private readonly TransactionRepository _sut;

        public TransactionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new TransactionRepository(_context);

            // Seed required entities
            var category = new Category { Id = 1, Name = "Category A" };
            var warehouse = new Warehouse { Id = 1, Name = "Warehouse A", Location = "Jakarta", Capacity = 1000 };
            var user = new User { Id = 1, Username = "admin", Email = "a@a.com", FullName = "Admin", PasswordHash = "hash", Role = UserRole.Admin };
            var product = new Product { Id = 1, Name = "Product A", SKU = "SKU-A", Price = 100, CategoryId = 1 };

            _context.Categories.Add(category);
            _context.Warehouses.Add(warehouse);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        // ============================================================
        // GetByIdAsync (inherited)
        // ============================================================

        [Fact]
        public async Task GetByIdAsync_Should_Return_Transaction_When_Found()
        {
            // Arrange
            var transaction = new InventoryTransaction
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 100,
                TotalAmount = 1000,
                CreatedByUserId = 1,
                TransactionDate = DateTime.UtcNow
            };
            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(transaction.Id, default);

            // Assert
            result.Should().NotBeNull();
            result!.Quantity.Should().Be(10);
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
        public async Task AddAsync_Should_Add_Transaction()
        {
            // Arrange
            var transaction = new InventoryTransaction
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 100,
                TotalAmount = 1000,
                CreatedByUserId = 1,
                TransactionDate = DateTime.UtcNow
            };

            // Act
            await _sut.AddAsync(transaction, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.InventoryTransactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);
            result.Should().NotBeNull();
        }

        // ============================================================
        // GetByProductAsync
        // ============================================================

        [Fact]
        public async Task GetByProductAsync_Should_Return_Transactions()
        {
            // Arrange
            _context.InventoryTransactions.AddRange(
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Inbound, Quantity = 10, UnitPrice = 100, TotalAmount = 1000, CreatedByUserId = 1, TransactionDate = DateTime.UtcNow },
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Outbound, Quantity = 5, UnitPrice = 100, TotalAmount = 500, CreatedByUserId = 1, TransactionDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByProductAsync(1);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByProductAsync_Should_Return_Empty_When_None()
        {
            // Act
            var result = await _sut.GetByProductAsync(999);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetByWarehouseAsync
        // ============================================================

        [Fact]
        public async Task GetByWarehouseAsync_Should_Return_Transactions()
        {
            // Arrange
            _context.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 100,
                TotalAmount = 1000,
                CreatedByUserId = 1,
                TransactionDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByWarehouseAsync(1);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByWarehouseAsync_Should_Return_Empty_When_None()
        {
            // Act
            var result = await _sut.GetByWarehouseAsync(999);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetByDateRangeAsync
        // ============================================================

        [Fact]
        public async Task GetByDateRangeAsync_Should_Return_In_Range()
        {
            // Arrange
            _context.InventoryTransactions.AddRange(
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Inbound, Quantity = 10, UnitPrice = 100, TotalAmount = 1000, CreatedByUserId = 1, TransactionDate = new DateTime(2024, 1, 15) },
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Inbound, Quantity = 20, UnitPrice = 100, TotalAmount = 2000, CreatedByUserId = 1, TransactionDate = new DateTime(2024, 6, 15) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByDateRangeAsync(new DateTime(2024, 1, 1), new DateTime(2024, 3, 31));

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByDateRangeAsync_Should_Return_Empty_When_No_Match()
        {
            // Act
            var result = await _sut.GetByDateRangeAsync(new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetByTypeAsync
        // ============================================================

        [Fact]
        public async Task GetByTypeAsync_Should_Return_By_Type()
        {
            // Arrange
            _context.InventoryTransactions.AddRange(
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Inbound, Quantity = 10, UnitPrice = 100, TotalAmount = 1000, CreatedByUserId = 1, TransactionDate = DateTime.UtcNow },
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Outbound, Quantity = 5, UnitPrice = 100, TotalAmount = 500, CreatedByUserId = 1, TransactionDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByTypeAsync(TransactionType.Inbound);

            // Assert
            result.Should().HaveCount(1);
            result.First().Type.Should().Be(TransactionType.Inbound);
        }

        [Fact]
        public async Task GetByTypeAsync_Should_Return_Empty_When_None()
        {
            // Act
            var result = await _sut.GetByTypeAsync(TransactionType.Outbound);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetTotalQuantityMovedAsync
        // ============================================================

        [Fact]
        public async Task GetTotalQuantityMovedAsync_Should_Return_Sum()
        {
            // Arrange
            _context.InventoryTransactions.AddRange(
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Inbound, Quantity = 10, UnitPrice = 100, TotalAmount = 1000, CreatedByUserId = 1, TransactionDate = new DateTime(2024, 1, 15) },
                new InventoryTransaction { ProductId = 1, WarehouseId = 1, Type = TransactionType.Outbound, Quantity = 20, UnitPrice = 100, TotalAmount = 2000, CreatedByUserId = 1, TransactionDate = new DateTime(2024, 1, 20) }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetTotalQuantityMovedAsync(1, new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

            // Assert
            result.Should().Be(30); // 10 + 20
        }

        [Fact]
        public async Task GetTotalQuantityMovedAsync_Should_Return_Zero_When_None()
        {
            // Act
            var result = await _sut.GetTotalQuantityMovedAsync(1, new DateTime(2020, 1, 1), new DateTime(2020, 12, 31));

            // Assert
            result.Should().Be(0);
        }

        // ============================================================
        // Dispose
        // ============================================================

        protected void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
