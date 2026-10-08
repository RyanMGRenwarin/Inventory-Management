using FluentAssertions;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class ProductTests
    {
        [Fact]
        public void Product_Should_Have_Default_Values()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            product.Name.Should().BeEmpty();
            product.SKU.Should().BeEmpty();
            product.Price.Should().Be(0);
            product.StockQuantity.Should().Be(0);
            product.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Product_Should_Allow_Setting_Properties()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Price = 99.99m,
                StockQuantity = 10,
                MinimumStockThreshold = 5,
                CategoryId = 1
            };

            // Assert
            product.Name.Should().Be("Test Product");
            product.SKU.Should().Be("TEST-001");
            product.Price.Should().Be(99.99m);
            product.StockQuantity.Should().Be(10);
        }

        [Fact]
        public void Product_Should_Have_Empty_Collections_By_Default()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            product.InventoryTransactions.Should().NotBeNull().And.BeEmpty();
        }
    }
}
