using FluentAssertions;
using InventoryManagement.Domain.Entities.Base;

namespace InventoryManagement.Domain.Tests.Entities
{
    public class BaseEntityTests
    {
        [Fact]
        public void BaseEntity_Should_Have_Default_Values()
        {
            // Arrange & Act
            var entity = new BaseEntity();

            // Assert
            entity.Id.Should().Be(0);
            entity.CreatedAt.Should().Be(default(DateTime));
            entity.UpdatedAt.Should().BeNull();
            entity.IsDeleted.Should().BeFalse();
        }
    }
}
