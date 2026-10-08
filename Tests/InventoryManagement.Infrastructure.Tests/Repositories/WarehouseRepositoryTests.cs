using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Tests.Repositories
{
    public class WarehouseRepositoryTests
    {
        private readonly AppDbContext _context;
        private readonly WarehouseRepository _sut;

        public WarehouseRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new WarehouseRepository(_context);
        }

        // ============================================================
        // GetByIdAsync (inherited)
        // ============================================================

        [Fact]
        public async Task GetByIdAsync_Should_Return_Warehouse_When_Found()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Main", Location = "Jakarta", Capacity = 1000 };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(warehouse.Id, default);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Main");
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
        public async Task AddAsync_Should_Add_Warehouse()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "New", Location = "Surabaya", Capacity = 500 };

            // Act
            await _sut.AddAsync(warehouse, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Warehouses.FirstOrDefaultAsync(w => w.Name == "New");
            result.Should().NotBeNull();
            result!.Location.Should().Be("Surabaya");
        }

        // ============================================================
        // UpdateAsync
        // ============================================================

        [Fact]
        public async Task UpdateAsync_Should_Update_Warehouse()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Old", Location = "Jakarta", Capacity = 1000 };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            // Act
            warehouse.Name = "Updated";
            warehouse.Capacity = 2000;
            await _sut.UpdateAsync(warehouse, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Warehouses.FindAsync(warehouse.Id);
            result!.Name.Should().Be("Updated");
            result.Capacity.Should().Be(2000);
        }

        // ============================================================
        // DeleteAsync / DeleteByIdAsync
        // ============================================================

        [Fact]
        public async Task DeleteAsync_Should_Remove_Warehouse()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Test", Location = "Jakarta", Capacity = 1000 };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteAsync(warehouse, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Warehouses.FindAsync(warehouse.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteByIdAsync_Should_Soft_Delete_Warehouse()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Test", Location = "Jakarta", Capacity = 1000 };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteByIdAsync(warehouse.Id, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Warehouses.IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.Id == warehouse.Id);

            if (result != null)
                result.IsDeleted.Should().BeTrue();
            else
                result.Should().BeNull();
        }

        // ============================================================
        // GetAvailableWarehousesAsync
        // ============================================================

        [Fact]
        public async Task GetAvailableWarehousesAsync_Should_Return_Only_Available()
        {
            // Arrange
            _context.Warehouses.AddRange(
                new Warehouse { Name = "Available", Location = "Jakarta", Capacity = 100, CurrentOccupancy = 50 },
                new Warehouse { Name = "Full", Location = "Bandung", Capacity = 100, CurrentOccupancy = 100 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetAvailableWarehousesAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Available");
        }

        [Fact]
        public async Task GetAvailableWarehousesAsync_Should_Return_Empty_When_All_Full()
        {
            // Arrange
            _context.Warehouses.Add(new Warehouse { Name = "Full", Location = "Jakarta", Capacity = 100, CurrentOccupancy = 100 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetAvailableWarehousesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // UpdateOccupancyAsync
        // ============================================================

        [Fact]
        public async Task UpdateOccupancyAsync_Should_Increase_Occupancy()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Main", Location = "Jakarta", Capacity = 1000, CurrentOccupancy = 100 };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateOccupancyAsync(warehouse.Id, 50);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Warehouses.FindAsync(warehouse.Id);
            result!.CurrentOccupancy.Should().Be(150);
        }

        [Fact]
        public async Task UpdateOccupancyAsync_Should_Decrease_Occupancy()
        {
            // Arrange
            var warehouse = new Warehouse { Name = "Main", Location = "Jakarta", Capacity = 1000, CurrentOccupancy = 100 };
            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            // Act
            await _sut.UpdateOccupancyAsync(warehouse.Id, -30);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Warehouses.FindAsync(warehouse.Id);
            result!.CurrentOccupancy.Should().Be(70);
        }

        [Fact]
        public async Task UpdateOccupancyAsync_Should_Not_Throw_When_Not_Found()
        {
            // Act
            var act = async () => await _sut.UpdateOccupancyAsync(999, 50);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ============================================================
        // GetHighOccupancyWarehousesAsync
        // ============================================================

        [Fact]
        public async Task GetHighOccupancyWarehousesAsync_Should_Return_High_Occupancy()
        {
            // Arrange
            _context.Warehouses.AddRange(
                new Warehouse { Name = "High", Location = "Jakarta", Capacity = 100, CurrentOccupancy = 95 },
                new Warehouse { Name = "Low", Location = "Bandung", Capacity = 100, CurrentOccupancy = 30 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetHighOccupancyWarehousesAsync(90);

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("High");
        }

        [Fact]
        public async Task GetHighOccupancyWarehousesAsync_Should_Return_Empty_When_None()
        {
            // Arrange
            _context.Warehouses.Add(new Warehouse { Name = "Low", Location = "Jakarta", Capacity = 100, CurrentOccupancy = 30 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetHighOccupancyWarehousesAsync(90);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // AnyAsync (inherited)
        // ============================================================

        [Fact]
        public async Task AnyAsync_Should_Return_True_When_Match()
        {
            // Arrange
            _context.Warehouses.Add(new Warehouse { Name = "Test", Location = "Jakarta", Capacity = 1000 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.AnyAsync(w => w.Name == "Test", default);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AnyAsync_Should_Return_False_When_No_Match()
        {
            // Act
            var result = await _sut.AnyAsync(w => w.Name == "Not-Exist", default);

            // Assert
            result.Should().BeFalse();
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
