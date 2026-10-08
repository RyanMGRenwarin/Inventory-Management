using FluentAssertions;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class WarehouseTests
    {
        [Fact]
        public void Warehouse_Should_Have_Default_Values()
        {
            // Arrange & Act
            var warehouse = new Warehouse();

            // Assert
            warehouse.Name.Should().BeEmpty();
            warehouse.Location.Should().BeEmpty();
            warehouse.Capacity.Should().Be(0);
            warehouse.CurrentOccupancy.Should().Be(0);
            warehouse.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Warehouse_Should_Allow_Setting_Properties()
        {
            // Arrange
            var warehouse = new Warehouse
            {
                Name = "Main Warehouse",
                Location = "Jakarta",
                Capacity = 1000,
                CurrentOccupancy = 250
            };

            // Assert
            warehouse.Name.Should().Be("Main Warehouse");
            warehouse.Location.Should().Be("Jakarta");
            warehouse.Capacity.Should().Be(1000);
            warehouse.CurrentOccupancy.Should().Be(250);
        }

        [Fact]
        public void Warehouse_Should_Have_Empty_InventoryTransactions_By_Default()
        {
            // Arrange & Act
            var warehouse = new Warehouse();

            // Assert
            warehouse.InventoryTransactions.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Warehouse_Occupancy_Can_Be_Updated()
        {
            // Arrange
            var warehouse = new Warehouse
            {
                Name = "Main",
                Location = "Jakarta",
                Capacity = 1000,
                CurrentOccupancy = 100
            };

            // Act
            warehouse.CurrentOccupancy += 50;

            // Assert
            warehouse.CurrentOccupancy.Should().Be(150);
        }
    }
}
