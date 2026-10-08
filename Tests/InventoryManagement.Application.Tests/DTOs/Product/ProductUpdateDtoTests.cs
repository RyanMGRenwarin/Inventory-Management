using FluentAssertions;
using InventoryManagement.Application.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Product
{
    public class ProductUpdateDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void ProductUpdateDto_Should_Be_Valid_When_All_Correct()
        {
            var dto = new ProductUpdateDto
            {
                Id = 1,
                Name = "Test",
                SKU = "TEST-001",
                Price = 99.99m,
                StockQuantity = 10,
                MinimumStockThreshold = 5,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void ProductUpdateDto_Should_Fail_When_Id_Zero()
        {
            var dto = new ProductUpdateDto
            {
                Id = 0,
                Name = "Test",
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Id"));
        }

        [Fact]
        public void ProductUpdateDto_Should_Fail_When_Name_Too_Short()
        {
            var dto = new ProductUpdateDto
            {
                Id = 1,
                Name = "A",
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void ProductUpdateDto_Should_Fail_When_CategoryId_Zero()
        {
            var dto = new ProductUpdateDto
            {
                Id = 1,
                Name = "Test",
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 0
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("CategoryId"));
        }

        [Fact]
        public void ProductUpdateDto_Should_Fail_When_SKU_Invalid_Pattern()
        {
            var dto = new ProductUpdateDto
            {
                Id = 1,
                Name = "Test",
                SKU = "invalid",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("SKU"));
        }
    }
}
