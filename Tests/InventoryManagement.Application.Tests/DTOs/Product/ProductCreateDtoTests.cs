using FluentAssertions;
using InventoryManagement.Application.DTOs.Product;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Product
{
    public class ProductCreateDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void ProductCreateDto_Should_Be_Valid_When_All_Fields_Correct()
        {
            var dto = new ProductCreateDto
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Description = "Description",
                Price = 99.99m,
                StockQuantity = 10,
                MinimumStockThreshold = 5,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_Name_Empty()
        {
            var dto = new ProductCreateDto
            {
                Name = "",
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_Name_Too_Short()
        {
            var dto = new ProductCreateDto
            {
                Name = "A",
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_Name_Too_Long()
        {
            var dto = new ProductCreateDto
            {
                Name = new string('A', 101),
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_SKU_Empty()
        {
            var dto = new ProductCreateDto
            {
                Name = "Test",
                SKU = "",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("SKU"));
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_SKU_Invalid_Pattern()
        {
            var dto = new ProductCreateDto
            {
                Name = "Test",
                SKU = "invalid-sku",
                Price = 99.99m,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("SKU"));
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_CategoryId_Zero()
        {
            var dto = new ProductCreateDto
            {
                Name = "Test",
                SKU = "TEST-001",
                Price = 99.99m,
                CategoryId = 0
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("CategoryId"));
        }

        [Fact]
        public void ProductCreateDto_Should_Fail_When_StockQuantity_Negative()
        {
            var dto = new ProductCreateDto
            {
                Name = "Test",
                SKU = "TEST-001",
                Price = 99.99m,
                StockQuantity = -1,
                CategoryId = 1
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("StockQuantity"));
        }
    }
}
