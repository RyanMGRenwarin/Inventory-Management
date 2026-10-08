using FluentAssertions;
using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.Infrastructure.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _loggerMock = new Mock<ILogger<CategoryService>>();

            _unitOfWorkMock.Setup(x => x.Categories).Returns(_categoryRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.Products).Returns(_productRepoMock.Object);

            _sut = new CategoryService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        // ============================================================
        // GetCategoriesAsync
        // ============================================================

        [Fact]
        public async Task GetCategoriesAsync_Should_Return_All_Categories()
        {
            // Arrange
            var filter = new CategoryFilterDto();
            var categoriesWithCounts = new List<(Category Category, int ProductCount)>
        {
            (new Category { Id = 1, Name = "Electronics" }, 5),
            (new Category { Id = 2, Name = "Clothing" }, 3)
        };

            _categoryRepoMock
                .Setup(x => x.GetCategoriesWithProductCountsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoriesWithCounts);

            // Act
            var result = await _sut.GetCategoriesAsync(filter);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCategoriesAsync_Should_Filter_By_SearchTerm()
        {
            // Arrange
            var filter = new CategoryFilterDto { SearchTerm = "elec" };
            var categoriesWithCounts = new List<(Category Category, int ProductCount)>
        {
            (new Category { Id = 1, Name = "Electronics" }, 5),
            (new Category { Id = 2, Name = "Clothing" }, 3)
        };

            _categoryRepoMock
                .Setup(x => x.GetCategoriesWithProductCountsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoriesWithCounts);

            // Act
            var result = await _sut.GetCategoriesAsync(filter);

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetCategoriesAsync_Should_Return_Empty_When_None()
        {
            // Arrange
            var filter = new CategoryFilterDto();

            _categoryRepoMock
                .Setup(x => x.GetCategoriesWithProductCountsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<(Category, int)>());

            // Act
            var result = await _sut.GetCategoriesAsync(filter);

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // GetCategoryByIdAsync
        // ============================================================

        [Fact]
        public async Task GetCategoryByIdAsync_Should_Return_When_Found()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _productRepoMock
                .Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            // Act
            var result = await _sut.GetCategoryByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Electronics");
            result.ProductCount.Should().Be(5);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.GetCategoryByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // CreateCategoryAsync
        // ============================================================

        [Fact]
        public async Task CreateCategoryAsync_Should_Create_When_Name_Unique()
        {
            // Arrange
            var createDto = new CategoryCreateDto { Name = "New", Description = "Desc" };

            _categoryRepoMock
                .Setup(x => x.NameExistsAsync("New", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.CreateCategoryAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New");

            _categoryRepoMock.Verify(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_Should_Throw_When_Name_Duplicate()
        {
            // Arrange
            var createDto = new CategoryCreateDto { Name = "Electronics" };

            _categoryRepoMock
                .Setup(x => x.NameExistsAsync("Electronics", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _sut.CreateCategoryAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        // ============================================================
        // UpdateCategoryAsync
        // ============================================================

        [Fact]
        public async Task UpdateCategoryAsync_Should_Update_When_Valid()
        {
            // Arrange
            var existing = new Category { Id = 1, Name = "Old" };
            var updateDto = new CategoryCreateDto { Name = "Updated", Description = "New Desc" };

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _categoryRepoMock
                .Setup(x => x.NameExistsAsync("Updated", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _productRepoMock
                .Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _sut.UpdateCategoryAsync(1, updateDto);

            // Assert
            result.Name.Should().Be("Updated");
            _categoryRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_Should_Throw_When_Not_Found()
        {
            // Arrange
            var updateDto = new CategoryCreateDto { Name = "Updated" };

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var act = async () => await _sut.UpdateCategoryAsync(999, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task UpdateCategoryAsync_Should_Throw_When_Name_Duplicate()
        {
            // Arrange
            var existing = new Category { Id = 1, Name = "Old" };
            var updateDto = new CategoryCreateDto { Name = "Duplicate" };

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _categoryRepoMock
                .Setup(x => x.NameExistsAsync("Duplicate", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _sut.UpdateCategoryAsync(1, updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        // ============================================================
        // DeleteCategoryAsync
        // ============================================================

        [Fact]
        public async Task DeleteCategoryAsync_Should_Delete_When_No_Products()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _productRepoMock
                .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteCategoryAsync(1);

            // Assert
            result.Should().BeTrue();
            _categoryRepoMock.Verify(x => x.DeleteByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_Should_Return_False_When_Not_Found()
        {
            // Arrange
            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _sut.DeleteCategoryAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteCategoryAsync_Should_Throw_When_Has_Products()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _productRepoMock
                .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _sut.DeleteCategoryAsync(1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*existing products*");
        }

        // ============================================================
        // GetAllCategoriesAsync
        // ============================================================

        [Fact]
        public async Task GetAllCategoriesAsync_Should_Return_All()
        {
            // Arrange
            var categoriesWithCounts = new List<(Category Category, int ProductCount)>
        {
            (new Category { Id = 1, Name = "A" }, 5),
            (new Category { Id = 2, Name = "B" }, 3)
        };

            _categoryRepoMock
                .Setup(x => x.GetCategoriesWithProductCountsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoriesWithCounts);

            // Act
            var result = await _sut.GetAllCategoriesAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        // ============================================================
        // IsCategoryNameAvailableAsync
        // ============================================================

        [Fact]
        public async Task IsCategoryNameAvailableAsync_Should_Return_True_When_Available()
        {
            // Arrange
            _categoryRepoMock
                .Setup(x => x.NameExistsAsync("New", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.IsCategoryNameAvailableAsync("New", default);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsCategoryNameAvailableAsync_Should_Return_False_When_Exists()
        {
            // Arrange
            _categoryRepoMock
                .Setup(x => x.NameExistsAsync("Existing", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.IsCategoryNameAvailableAsync("Existing", default);

            // Assert
            result.Should().BeFalse();
        }
    }
}
