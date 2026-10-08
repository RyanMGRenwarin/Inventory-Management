using FluentAssertions;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.Infrastructure.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
        private readonly Mock<IDatabaseTransaction> _dbTransactionMock;
        private readonly Mock<ILogger<TransactionService>> _loggerMock;
        private readonly TransactionService _sut;

        public TransactionServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _warehouseRepoMock = new Mock<IWarehouseRepository>();
            _dbTransactionMock = new Mock<IDatabaseTransaction>();
            _loggerMock = new Mock<ILogger<TransactionService>>();

            _unitOfWorkMock.Setup(x => x.Transactions).Returns(_transactionRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.Products).Returns(_productRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.Warehouses).Returns(_warehouseRepoMock.Object);

            _unitOfWorkMock
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbTransactionMock.Object);

            _sut = new TransactionService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        // ============================================================
        // GetTransactionsAsync
        // ============================================================

        [Fact]
        public async Task GetTransactionsAsync_Should_Return_Paginated_List()
        {
            // Arrange
            var filter = new TransactionFilterDto { PageNumber = 1, PageSize = 10 };
            var transactions = new List<InventoryTransaction>
        {
            new InventoryTransaction { Id = 1, ProductId = 1, WarehouseId = 1, Type = TransactionType.Inbound, Quantity = 10, UnitPrice = 100m, TotalAmount = 1000m },
            new InventoryTransaction { Id = 2, ProductId = 1, WarehouseId = 1, Type = TransactionType.Outbound, Quantity = 5, UnitPrice = 100m, TotalAmount = 500m }
        };

            _transactionRepoMock
                .Setup(x => x.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<InventoryTransaction, bool>>>(),
                    It.IsAny<Func<IQueryable<InventoryTransaction>, IOrderedQueryable<InventoryTransaction>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((transactions, 2));

            // Act
            var result = await _sut.GetTransactionsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Transactions.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTransactionsAsync_Should_Return_Empty_When_None()
        {
            // Arrange
            var filter = new TransactionFilterDto();

            _transactionRepoMock
                .Setup(x => x.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<InventoryTransaction, bool>>>(),
                    It.IsAny<Func<IQueryable<InventoryTransaction>, IOrderedQueryable<InventoryTransaction>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<InventoryTransaction>(), 0));

            // Act
            var result = await _sut.GetTransactionsAsync(filter);

            // Assert
            result.Transactions.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        // ============================================================
        // GetTransactionByIdAsync
        // ============================================================

        [Fact]
        public async Task GetTransactionByIdAsync_Should_Return_When_Found()
        {
            // Arrange
            var transaction = new InventoryTransaction { Id = 1, ProductId = 1, WarehouseId = 1, Quantity = 10 };

            _transactionRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction);

            // Act
            var result = await _sut.GetTransactionByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            _transactionRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryTransaction?)null);

            // Act
            var result = await _sut.GetTransactionByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // CreateTransactionAsync - Inbound
        // ============================================================

        [Fact]
        public async Task CreateTransactionAsync_Inbound_Should_Increase_Stock()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", StockQuantity = 100, CategoryId = 1 };
            var warehouse = new Warehouse { Id = 1, Name = "Main", Capacity = 1000, CurrentOccupancy = 100 };

            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 50,
                UnitPrice = 100m,
                Notes = "Test"
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _warehouseRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

            // Act
            var result = await _sut.CreateTransactionAsync(dto, userId: 1);

            // Assert
            result.Should().NotBeNull();
            result.Quantity.Should().Be(50);
            result.TotalAmount.Should().Be(5000m);

            _productRepoMock.Verify(x => x.UpdateStockAsync(1, 150, It.IsAny<CancellationToken>()), Times.Once);
            _warehouseRepoMock.Verify(x => x.UpdateOccupancyAsync(1, 50, It.IsAny<CancellationToken>()), Times.Once);
            _dbTransactionMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // ============================================================
        // CreateTransactionAsync - Outbound
        // ============================================================

        [Fact]
        public async Task CreateTransactionAsync_Outbound_Should_Decrease_Stock()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", StockQuantity = 100, CategoryId = 1 };
            var warehouse = new Warehouse { Id = 1, Name = "Main", Capacity = 1000, CurrentOccupancy = 100 };

            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Outbound,
                Quantity = 30,
                UnitPrice = 100m
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _warehouseRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

            // Act
            var result = await _sut.CreateTransactionAsync(dto, userId: 1);

            // Assert
            result.Should().NotBeNull();
            _productRepoMock.Verify(x => x.UpdateStockAsync(1, 70, It.IsAny<CancellationToken>()), Times.Once);
            _warehouseRepoMock.Verify(x => x.UpdateOccupancyAsync(1, -30, It.IsAny<CancellationToken>()), Times.Once);
        }

        // ============================================================
        // CreateTransactionAsync - Validation
        // ============================================================

        [Fact]
        public async Task CreateTransactionAsync_Should_Throw_When_Product_Not_Found()
        {
            // Arrange
            var dto = new TransactionCreateDto { ProductId = 999, WarehouseId = 1, Quantity = 10, UnitPrice = 100m };

            _productRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

            // Act
            var act = async () => await _sut.CreateTransactionAsync(dto, userId: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Product*does not exist*");
        }

        [Fact]
        public async Task CreateTransactionAsync_Should_Throw_When_Warehouse_Not_Found()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", StockQuantity = 100, CategoryId = 1 };
            var dto = new TransactionCreateDto { ProductId = 1, WarehouseId = 999, Quantity = 10, UnitPrice = 100m };

            _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _warehouseRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

            // Act
            var act = async () => await _sut.CreateTransactionAsync(dto, userId: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Warehouse*does not exist*");
        }

        [Fact]
        public async Task CreateTransactionAsync_Should_Throw_When_Insufficient_Stock_For_Outbound()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", StockQuantity = 5, CategoryId = 1 };
            var warehouse = new Warehouse { Id = 1, Name = "Main", Capacity = 1000, CurrentOccupancy = 100 };

            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Outbound,
                Quantity = 100, // > stock
                UnitPrice = 100m
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _warehouseRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

            // Act
            var act = async () => await _sut.CreateTransactionAsync(dto, userId: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Insufficient stock*");
        }

        [Fact]
        public async Task CreateTransactionAsync_Should_Throw_When_Warehouse_Capacity_Exceeded()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", StockQuantity = 100, CategoryId = 1 };
            var warehouse = new Warehouse { Id = 1, Name = "Main", Capacity = 100, CurrentOccupancy = 90 };

            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 50, // Would exceed capacity
                UnitPrice = 100m
            };

            _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _warehouseRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

            // Act
            var act = async () => await _sut.CreateTransactionAsync(dto, userId: 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*capacity exceeded*");
        }

        // ============================================================
        // DeleteTransactionAsync
        // ============================================================

        [Fact]
        public async Task DeleteTransactionAsync_Should_Reverse_Stock_And_Occupancy()
        {
            // Arrange
            var transaction = new InventoryTransaction
            {
                Id = 1,
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 50,
                UnitPrice = 100m
            };
            var product = new Product { Id = 1, StockQuantity = 150, CategoryId = 1 };
            var warehouse = new Warehouse { Id = 1, Capacity = 1000, CurrentOccupancy = 150 };

            _transactionRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
            _productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _warehouseRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

            // Act
            var result = await _sut.DeleteTransactionAsync(1);

            // Assert
            result.Should().BeTrue();
            _productRepoMock.Verify(x => x.UpdateStockAsync(1, 100, It.IsAny<CancellationToken>()), Times.Once);
            _warehouseRepoMock.Verify(x => x.UpdateOccupancyAsync(1, -50, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTransactionAsync_Should_Return_False_When_Not_Found()
        {
            // Arrange
            _transactionRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((InventoryTransaction?)null);

            // Act
            var result = await _sut.DeleteTransactionAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // GetTransactionSummaryAsync
        // ============================================================

        [Fact]
        public async Task GetTransactionSummaryAsync_Should_Return_Correct_Summary()
        {
            // Arrange
            var transactions = new List<InventoryTransaction>
        {
            new InventoryTransaction { Id = 1, Type = TransactionType.Inbound, Quantity = 100, TotalAmount = 10000m, TransactionDate = DateTime.UtcNow },
            new InventoryTransaction { Id = 2, Type = TransactionType.Outbound, Quantity = 50, TotalAmount = 5000m, TransactionDate = DateTime.UtcNow }
        };

            _transactionRepoMock
                .Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _sut.GetTransactionSummaryAsync();

            // Assert
            result.TotalTransactions.Should().Be(2);
            result.TotalInboundQuantity.Should().Be(100);
            result.TotalOutboundQuantity.Should().Be(50);
            result.TotalValue.Should().Be(15000m);
        }

        [Fact]
        public async Task GetTransactionSummaryAsync_Should_Return_Empty_When_None()
        {
            // Arrange
            _transactionRepoMock
                .Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryTransaction>());

            // Act
            var result = await _sut.GetTransactionSummaryAsync();

            // Assert
            result.TotalTransactions.Should().Be(0);
            result.TotalValue.Should().Be(0);
        }
    }
}
