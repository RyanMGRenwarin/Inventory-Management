using FluentAssertions;
using InventoryManagement.Application.DTOs.Category;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.Tests.DTOs.Category
{
    public class CategoryCreateDtoTests
    {
        private static List<ValidationResult> Validate(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }

        [Fact]
        public void CategoryCreateDto_Should_Be_Valid_When_All_Correct()
        {
            var dto = new CategoryCreateDto
            {
                Name = "Electronics",
                Description = "Electronic devices"
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void CategoryCreateDto_Should_Be_Valid_Without_Description()
        {
            var dto = new CategoryCreateDto
            {
                Name = "Electronics",
                Description = ""
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void CategoryCreateDto_Should_Fail_When_Name_Empty()
        {
            var dto = new CategoryCreateDto
            {
                Name = "",
                Description = "Test"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CategoryCreateDto_Should_Fail_When_Name_Too_Short()
        {
            var dto = new CategoryCreateDto
            {
                Name = "A",
                Description = "Test"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CategoryCreateDto_Should_Fail_When_Name_Too_Long()
        {
            var dto = new CategoryCreateDto
            {
                Name = new string('A', 51),
                Description = "Test"
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CategoryCreateDto_Should_Fail_When_Description_Too_Long()
        {
            var dto = new CategoryCreateDto
            {
                Name = "Test",
                Description = new string('A', 201)
            };

            var results = Validate(dto);
            results.Should().Contain(r => r.MemberNames.Contains("Description"));
        }
    }
}
