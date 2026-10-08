using FluentAssertions;
using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.Infrastructure.Tests.Services
{
    public class WarehouseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<ILogger<WarehouseService>> _loggerMock;
        private readonly WarehouseService _sut;

        public WarehouseServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _warehouseRepoMock = new Mock<IWarehouseRepository>();
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _loggerMock = new Mock<ILogger<WarehouseService>>();

            _unitOfWorkMock.Setup(x => x.Warehouses).Returns(_warehouseRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.Transactions).Returns(_transactionRepoMock.Object);

            _sut = new WarehouseService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        // ============================================================
        // GetWarehousesAsync
        // ============================================================

        [Fact]
        public async Task GetWarehousesAsync_Should_Return_All_Warehouses()
        {
            // Arrange
            var filter = new WarehouseFilterDto();
            var warehouses = new List<Warehouse>
            {
                new() { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000, CurrentOccupancy = 100 },
                new() { Id = 2, Name = "Secondary", Location = "Bandung", Capacity = 500, CurrentOccupancy = 50 }
            };

            _warehouseRepoMock
                .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Warehouse, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouses);

            // Act
            var result = await _sut.GetWarehousesAsync(filter);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetWarehousesAsync_Should_Filter_By_SearchTerm()
        {
            // Arrange
            var filter = new WarehouseFilterDto { SearchTerm = "jakarta" };
            var warehouses = new List<Warehouse>
        {
            new Warehouse { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000 }
        };

            _warehouseRepoMock
                .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Warehouse, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouses);

            // Act
            var result = await _sut.GetWarehousesAsync(filter);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetWarehousesAsync_Should_Return_Empty_When_None()
        {
            // Arrange
            var filter = new WarehouseFilterDto();

            _warehouseRepoMock
                .Setup(x => x.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Warehouse, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Warehouse>());

            // Act
            var result = await _sut.GetWarehousesAsync(filter);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetWarehouseByIdAsync
        // ============================================================

        [Fact]
        public async Task GetWarehouseByIdAsync_Should_Return_Warehouse_When_Found()
        {
            // Arrange
            var warehouse = new Warehouse { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000 };

            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            // Act
            var result = await _sut.GetWarehouseByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Main");
        }

        [Fact]
        public async Task GetWarehouseByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Warehouse?)null);

            // Act
            var result = await _sut.GetWarehouseByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // CreateWarehouseAsync
        // ============================================================

        [Fact]
        public async Task CreateWarehouseAsync_Should_Create_Successfully()
        {
            // Arrange
            var createDto = new WarehouseCreateDto
            {
                Name = "New Warehouse",
                Location = "Surabaya",
                Capacity = 500
            };

            // Act
            var result = await _sut.CreateWarehouseAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Warehouse");
            result.Location.Should().Be("Surabaya");
            result.Capacity.Should().Be(500);

            _warehouseRepoMock.Verify(x => x.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // ============================================================
        // UpdateWarehouseAsync
        // ============================================================

        [Fact]
        public async Task UpdateWarehouseAsync_Should_Update_Successfully()
        {
            // Arrange
            var existing = new Warehouse { Id = 1, Name = "Old", Location = "Jakarta", Capacity = 1000 };
            var updateDto = new WarehouseCreateDto { Name = "Updated", Location = "Bandung", Capacity = 2000 };

            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            // Act
            var result = await _sut.UpdateWarehouseAsync(1, updateDto);

            // Assert
            result.Name.Should().Be("Updated");
            result.Location.Should().Be("Bandung");
            result.Capacity.Should().Be(2000);

            _warehouseRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateWarehouseAsync_Should_Throw_When_Not_Found()
        {
            // Arrange
            var updateDto = new WarehouseCreateDto { Name = "Updated", Location = "Bandung", Capacity = 2000 };

            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Warehouse?)null);

            // Act
            var act = async () => await _sut.UpdateWarehouseAsync(999, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*does not exist*");
        }

        // ============================================================
        // DeleteWarehouseAsync
        // ============================================================

        [Fact]
        public async Task DeleteWarehouseAsync_Should_Delete_When_No_Transactions()
        {
            // Arrange
            var warehouse = new Warehouse { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000 };

            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            _transactionRepoMock
                .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryTransaction, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteWarehouseAsync(1);

            // Assert
            result.Should().BeTrue();
            _warehouseRepoMock.Verify(x => x.DeleteByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteWarehouseAsync_Should_Return_False_When_Not_Found()
        {
            // Arrange
            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Warehouse?)null);

            // Act
            var result = await _sut.DeleteWarehouseAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteWarehouseAsync_Should_Throw_When_Has_Transactions()
        {
            // Arrange
            var warehouse = new Warehouse { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000 };

            _warehouseRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            _transactionRepoMock
                .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<InventoryTransaction, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _sut.DeleteWarehouseAsync(1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*existing transactions*");
        }

        // ============================================================
        // GetAllWarehousesAsync
        // ============================================================

        [Fact]
        public async Task GetAllWarehousesAsync_Should_Return_All()
        {
            // Arrange
            _warehouseRepoMock
                .Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Warehouse>
                {
                new Warehouse { Id = 1, Name = "A", Location = "X", Capacity = 100 },
                new Warehouse { Id = 2, Name = "B", Location = "Y", Capacity = 200 }
                });

            // Act
            var result = await _sut.GetAllWarehousesAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        // ============================================================
        // GetAvailableWarehousesAsync
        // ============================================================

        [Fact]
        public async Task GetAvailableWarehousesAsync_Should_Return_Available()
        {
            // Arrange
            _warehouseRepoMock
                .Setup(x => x.GetAvailableWarehousesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Warehouse>
                {
                new Warehouse { Id = 1, Name = "A", Location = "X", Capacity = 100, CurrentOccupancy = 50 }
                });

            // Act
            var result = await _sut.GetAvailableWarehousesAsync();

            // Assert
            result.Should().HaveCount(1);
        }
    }
}
