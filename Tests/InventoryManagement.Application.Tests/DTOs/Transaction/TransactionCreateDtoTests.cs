using FluentAssertions;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Transaction
{
    public class TransactionCreateDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void TransactionCreateDto_Should_Be_Valid_When_All_Correct()
        {
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 99.99m,
                Notes = "Test transaction"
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void TransactionCreateDto_Should_Fail_When_ProductId_Zero()
        {
            var dto = new TransactionCreateDto
            {
                ProductId = 0,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 99.99m
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("ProductId"));
        }

        [Fact]
        public void TransactionCreateDto_Should_Fail_When_WarehouseId_Zero()
        {
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 0,
                Type = TransactionType.Inbound,
                Quantity = 10,
                UnitPrice = 99.99m
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("WarehouseId"));
        }

        [Fact]
        public void TransactionCreateDto_Should_Fail_When_Quantity_Zero()
        {
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = 0,
                UnitPrice = 99.99m
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Quantity"));
        }

        [Fact]
        public void TransactionCreateDto_Should_Fail_When_Quantity_Negative()
        {
            var dto = new TransactionCreateDto
            {
                ProductId = 1,
                WarehouseId = 1,
                Type = TransactionType.Inbound,
                Quantity = -5,
                UnitPrice = 99.99m
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Quantity"));
        }
    }
}
