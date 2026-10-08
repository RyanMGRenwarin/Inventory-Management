using FluentAssertions;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Tests.Repositories
{
    public class CategoryRepositoryTests
    {
        private readonly AppDbContext _context;
        private readonly CategoryRepository _sut;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new CategoryRepository(_context);
        }

        // ============================================================
        // GetByIdAsync (inherited)
        // ============================================================

        [Fact]
        public async Task GetByIdAsync_Should_Return_Category_When_Found()
        {
            // Arrange
            var category = new Category { Name = "Electronics", Description = "Electronic devices" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(category.Id, default);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetByIdAsync(999, default);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // AddAsync
        // ============================================================

        [Fact]
        public async Task AddAsync_Should_Add_Category()
        {
            // Arrange
            var category = new Category { Name = "New", Description = "Desc" };

            // Act
            await _sut.AddAsync(category, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "New");
            result.Should().NotBeNull();
        }

        // ============================================================
        // UpdateAsync
        // ============================================================

        [Fact]
        public async Task UpdateAsync_Should_Update_Category()
        {
            // Arrange
            var category = new Category { Name = "Old", Description = "Old Desc" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            category.Name = "Updated";
            category.Description = "New Desc";
            await _sut.UpdateAsync(category, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Categories.FindAsync(category.Id);
            result!.Name.Should().Be("Updated");
            result.Description.Should().Be("New Desc");
        }

        // ============================================================
        // DeleteAsync / DeleteByIdAsync
        // ============================================================

        [Fact]
        public async Task DeleteAsync_Should_Remove_Category()
        {
            // Arrange
            var category = new Category { Name = "Test" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteAsync(category, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Categories.FindAsync(category.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteByIdAsync_Should_Soft_Delete_Category()
        {
            // Arrange
            var category = new Category { Name = "Test" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            await _sut.DeleteByIdAsync(category.Id, default);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Categories.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            if (result != null)
                result.IsDeleted.Should().BeTrue();
            else
                result.Should().BeNull();
        }

        // ============================================================
        // GetByNameAsync
        // ============================================================

        [Fact]
        public async Task GetByNameAsync_Should_Return_Category_When_Found()
        {
            // Arrange
            _context.Categories.Add(new Category { Name = "Electronics" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByNameAsync("Electronics");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetByNameAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _sut.GetByNameAsync("NotExist");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByNameAsync_Should_Exclude_Soft_Deleted()
        {
            // Arrange
            _context.Categories.Add(new Category { Name = "Deleted", IsDeleted = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByNameAsync("Deleted");

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // GetCategoriesWithProductCountsAsync
        // ============================================================

        [Fact]
        public async Task GetCategoriesWithProductCountsAsync_Should_Return_Counts()
        {
            // Arrange
            var cat1 = new Category { Name = "Electronics" };
            var cat2 = new Category { Name = "Clothing" };
            _context.Categories.AddRange(cat1, cat2);
            await _context.SaveChangesAsync();

            _context.Products.AddRange(
                new Product { Name = "P1", SKU = "P1", Price = 10, CategoryId = cat1.Id },
                new Product { Name = "P2", SKU = "P2", Price = 20, CategoryId = cat1.Id },
                new Product { Name = "P3", SKU = "P3", Price = 30, CategoryId = cat2.Id }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetCategoriesWithProductCountsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First(c => c.Category.Name == "Electronics").ProductCount.Should().Be(2);
            result.First(c => c.Category.Name == "Clothing").ProductCount.Should().Be(1);
        }

        [Fact]
        public async Task GetCategoriesWithProductCountsAsync_Should_Return_Zero_Count()
        {
            // Arrange
            _context.Categories.Add(new Category { Name = "Empty" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetCategoriesWithProductCountsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().ProductCount.Should().Be(0);
        }

        // ============================================================
        // NameExistsAsync
        // ============================================================

        [Fact]
        public async Task NameExistsAsync_Should_Return_True_When_Exists()
        {
            // Arrange
            _context.Categories.Add(new Category { Name = "Electronics" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.NameExistsAsync("Electronics");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NameExistsAsync_Should_Return_False_When_Not_Exists()
        {
            // Act
            var result = await _sut.NameExistsAsync("NotExist");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameExistsAsync_Should_Exclude_Specified_CategoryId()
        {
            // Arrange
            var category = new Category { Name = "Electronics" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.NameExistsAsync("Electronics", excludeCategoryId: category.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameExistsAsync_Should_Exclude_Soft_Deleted()
        {
            // Arrange
            _context.Categories.Add(new Category { Name = "Deleted", IsDeleted = true });
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.NameExistsAsync("Deleted");

            // Assert
            result.Should().BeFalse();
        }

        // ============================================================
        // Dispose
        // ============================================================

        protected void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
