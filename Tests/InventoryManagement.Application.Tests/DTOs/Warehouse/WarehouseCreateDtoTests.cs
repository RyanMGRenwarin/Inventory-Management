using FluentAssertions;
using InventoryManagement.Application.DTOs.Warehouse;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Warehouse
{
    public class WarehouseCreateDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void WarehouseCreateDto_Should_Be_Valid_When_All_Correct()
        {
            var dto = new WarehouseCreateDto
            {
                Name = "Main Warehouse",
                Location = "Jakarta",
                Capacity = 1000
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void WarehouseCreateDto_Should_Fail_When_Name_Empty()
        {
            var dto = new WarehouseCreateDto
            {
                Name = "",
                Location = "Jakarta",
                Capacity = 1000
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void WarehouseCreateDto_Should_Fail_When_Name_Too_Short()
        {
            var dto = new WarehouseCreateDto
            {
                Name = "A",
                Location = "Jakarta",
                Capacity = 1000
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void WarehouseCreateDto_Should_Fail_When_Location_Empty()
        {
            var dto = new WarehouseCreateDto
            {
                Name = "Main",
                Location = "",
                Capacity = 1000
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Location"));
        }

        [Fact]
        public void WarehouseCreateDto_Should_Fail_When_Capacity_Zero()
        {
            var dto = new WarehouseCreateDto
            {
                Name = "Main",
                Location = "Jakarta",
                Capacity = 0
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Capacity"));
        }

        [Fact]
        public void WarehouseCreateDto_Should_Fail_When_Capacity_Negative()
        {
            var dto = new WarehouseCreateDto
            {
                Name = "Main",
                Location = "Jakarta",
                Capacity = -1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Capacity"));
        }
    }
}
