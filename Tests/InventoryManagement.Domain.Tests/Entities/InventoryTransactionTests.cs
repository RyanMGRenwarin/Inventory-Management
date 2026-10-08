using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class InventoryTransactionTests
    {
        [Fact]
        public void InventoryTransaction_Should_Have_Default_Values()
        {
            // Arrange & Act
            var transaction = new InventoryTransaction();

            // Assert
            transaction.ProductId.Should().Be(0);
            transaction.WarehouseId.Should().Be(0);
            transaction.CreatedByUserId.Should().Be(0);
            transaction.Quantity.Should().Be(0);
            transaction.UnitPrice.Should().Be(0);
            transaction.TotalAmount.Should().Be(0);
            transaction.Notes.Should().BeEmpty();
            transaction.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void InventoryTransaction_Should_Allow_Setting_Properties()
        {
            // Arrange & Act
            var transaction = new InventoryTransaction
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 100m,
                TotalAmount = 1000m,
                Notes = "Test transaction",
                CreatedByUserId = 1,
                TransactionDate = DateTime.UtcNow
            };

            // Assert
            transaction.ProductId.Should().Be(1);
            transaction.WarehouseId.Should().Be(1);
            transaction.Type.Should().Be(TransactionType.Inbound);
            transaction.Quantity.Should().Be(10);
            transaction.UnitPrice.Should().Be(100m);
            transaction.TotalAmount.Should().Be(1000m);
            transaction.Notes.Should().Be("Test transaction");
        }

        [Fact]
        public void InventoryTransaction_Inbound_Should_Have_Correct_Type()
        {
            // Arrange & Act
            var transaction = new InventoryTransaction
            {
                Type = TransactionType.Inbound
            };

            // Assert
            transaction.Type.Should().Be(TransactionType.Inbound);
            transaction.Type.Should().NotBe(TransactionType.Outbound);
        }

        [Fact]
        public void InventoryTransaction_Outbound_Should_Have_Correct_Type()
        {
            // Arrange & Act
            var transaction = new InventoryTransaction
            {
                Type = TransactionType.Outbound
            };

            // Assert
            transaction.Type.Should().Be(TransactionType.Outbound);
            transaction.Type.Should().NotBe(TransactionType.Inbound);
        }

        [Fact]
        public void InventoryTransaction_TotalAmount_Should_Be_Calculable()
        {
            // Arrange
            var transaction = new InventoryTransaction
            {
                Quantity = 10,
                UnitPrice = 99.99m
            };

            // Act
            transaction.TotalAmount = transaction.Quantity * transaction.UnitPrice;

            // Assert
            transaction.TotalAmount.Should().Be(999.90m);
        }
    }
}
