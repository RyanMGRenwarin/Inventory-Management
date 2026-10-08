using FluentAssertions;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class CategoryTests
    {
        [Fact]
        public void Category_Should_Have_Default_Values()
        {
            // Arrange & Act
            var category = new Category();

            // Assert
            category.Name.Should().BeEmpty();
            category.Description.Should().BeEmpty();
            category.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Category_Should_Allow_Setting_Properties()
        {
            // Arrange
            var category = new Category
            {
                Name = "Electronics",
                Description = "Electronic devices"
            };

            // Assert
            category.Name.Should().Be("Electronics");
            category.Description.Should().Be("Electronic devices");
        }

        [Fact]
        public void Category_Should_Have_Empty_Products_Collection_By_Default()
        {
            // Arrange & Act
            var category = new Category();

            // Assert
            category.Products.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Category_Should_Allow_Adding_Products()
        {
            // Arrange
            var category = new Category { Name = "Electronics" };
            var product = new Product { Name = "Laptop", SKU = "LAP-001" };

            // Act
            category.Products.Add(product);

            // Assert
            category.Products.Should().ContainSingle();
            category.Products.First().Name.Should().Be("Laptop");
        }
    }
}
