using FluentAssertions;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.Infrastructure.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly ProductService _sut;

        public ProductServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _productRepoMock = new Mock<IProductRepository>();
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _loggerMock = new Mock<ILogger<ProductService>>();

            _unitOfWorkMock.Setup(x => x.Products).Returns(_productRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.Categories).Returns(_categoryRepoMock.Object);

            _sut = new ProductService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        // ============================================================
        // GetProductsAsync
        // ============================================================

        [Fact]
        public async Task GetProductsAsync_Should_Return_Paginated_List()
        {
            // Arrange
            var filter = new ProductFilterDto { PageNumber = 1, PageSize = 10 };
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product A", SKU = "SKU-A", Price = 10m, CategoryId = 1 },
            new Product { Id = 2, Name = "Product B", SKU = "SKU-B", Price = 20m, CategoryId = 1 }
        };

            _productRepoMock
                .Setup(x => x.GetPagedAsync(
                    filter.PageNumber,
                    filter.PageSize,
                    It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((products, 2));

            // Act
            var result = await _sut.GetProductsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.Products.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetProductsAsync_Should_Return_Empty_List_When_No_Products()
        {
            // Arrange
            var filter = new ProductFilterDto { PageNumber = 1, PageSize = 10 };

            _productRepoMock
                .Setup(x => x.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Product>(), 0));

            // Act
            var result = await _sut.GetProductsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            result.Products.Should().BeEmpty();
            result.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task GetProductsAsync_Should_Calculate_TotalPages_Correctly()
        {
            // Arrange
            var filter = new ProductFilterDto { PageNumber = 1, PageSize = 10 };

            _productRepoMock
                .Setup(x => x.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<Product>(), 25)); // 25 items / 10 per page = 3 pages

            // Act
            var result = await _sut.GetProductsAsync(filter);

            // Assert
            result.TotalPages.Should().Be(3);
            result.HasNextPage.Should().BeTrue();
            result.HasPreviousPage.Should().BeFalse();
        }

        // ============================================================
        // GetProductByIdAsync
        // ============================================================

        [Fact]
        public async Task GetProductByIdAsync_Should_Return_Product_When_Found()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test",
                SKU = "TEST-001",
                Price = 100m,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Category A" }
            };

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.GetProductByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Test");
            result.Price.Should().Be(100m);
        }

        [Fact]
        public async Task GetProductByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _sut.GetProductByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductByIdAsync_Should_Return_Null_When_Id_Is_Zero()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetByIdAsync(0, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _sut.GetProductByIdAsync(0);

            // Assert
            result.Should().BeNull();
        }

        // ============================================================
        // CreateProductAsync
        // ============================================================

        [Fact]
        public async Task CreateProductAsync_Should_Create_Product_When_SKU_Unique()
        {
            // Arrange
            var createDto = new ProductCreateDto
            {
                Name = "New Product",
                SKU = "NEW-001",
                Description = "Description",
                Price = 99.99m,
                StockQuantity = 10,
                MinimumStockThreshold = 5,
                CategoryId = 1
            };

            _productRepoMock
                .Setup(x => x.SkuExistsAsync("NEW-001", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { Id = 1, Name = "Category A" });

            // Act
            var result = await _sut.CreateProductAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Product");
            result.SKU.Should().Be("NEW-001");
            result.Price.Should().Be(99.99m);

            _productRepoMock.Verify(
                x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_Should_Throw_When_SKU_Duplicate()
        {
            // Arrange
            var createDto = new ProductCreateDto
            {
                Name = "New Product",
                SKU = "DUPLICATE",
                Price = 99.99m,
                CategoryId = 1
            };

            _productRepoMock
                .Setup(x => x.SkuExistsAsync("DUPLICATE", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _sut.CreateProductAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task CreateProductAsync_Should_Throw_When_Category_Not_Found()
        {
            // Arrange
            var createDto = new ProductCreateDto
            {
                Name = "New Product",
                SKU = "NEW-001",
                Price = 99.99m,
                CategoryId = 999
            };

            _productRepoMock
                .Setup(x => x.SkuExistsAsync("NEW-001", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var act = async () => await _sut.CreateProductAsync(createDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Category*does not exist*");
        }

        // ============================================================
        // UpdateProductAsync
        // ============================================================

        [Fact]
        public async Task UpdateProductAsync_Should_Update_Product_When_Valid()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Old Name",
                SKU = "OLD-001",
                Price = 50m,
                CategoryId = 1
            };

            var updateDto = new ProductUpdateDto
            {
                Id = 1,
                Name = "Updated Name",
                SKU = "UPD-001",
                Price = 75m,
                StockQuantity = 20,
                MinimumStockThreshold = 5,
                CategoryId = 1
            };

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _productRepoMock
                .Setup(x => x.SkuExistsAsync("UPD-001", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Category { Id = 1, Name = "Category A" });

            // Act
            var result = await _sut.UpdateProductAsync(updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.SKU.Should().Be("UPD-001");
            result.Price.Should().Be(75m);

            _productRepoMock.Verify(
                x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_Should_Throw_When_Product_Not_Found()
        {
            // Arrange
            var updateDto = new ProductUpdateDto { Id = 999, Name = "Test", SKU = "TEST" };

            _productRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var act = async () => await _sut.UpdateProductAsync(updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*does not exist*");
        }

        [Fact]
        public async Task UpdateProductAsync_Should_Throw_When_SKU_Duplicate()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Test",
                SKU = "OLD-001",
                Price = 50m,
                CategoryId = 1
            };

            var updateDto = new ProductUpdateDto
            {
                Id = 1,
                Name = "Updated",
                SKU = "DUPLICATE",
                Price = 75m,
                CategoryId = 1
            };

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _productRepoMock
                .Setup(x => x.SkuExistsAsync("DUPLICATE", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var act = async () => await _sut.UpdateProductAsync(updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task UpdateProductAsync_Should_Throw_When_Category_Not_Found()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = 1,
                Name = "Test",
                SKU = "OLD-001",
                Price = 50m,
                CategoryId = 1
            };

            var updateDto = new ProductUpdateDto
            {
                Id = 1,
                Name = "Updated",
                SKU = "UPD-001",
                Price = 75m,
                CategoryId = 999
            };

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _productRepoMock
                .Setup(x => x.SkuExistsAsync("UPD-001", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _categoryRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            var act = async () => await _sut.UpdateProductAsync(updateDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Category*does not exist*");
        }

        // ============================================================
        // DeleteProductAsync
        // ============================================================

        [Fact]
        public async Task DeleteProductAsync_Should_Return_True_When_Product_Exists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", SKU = "TEST-001", CategoryId = 1 };

            _productRepoMock
                .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.DeleteProductAsync(1);

            // Assert
            result.Should().BeTrue();

            _productRepoMock.Verify(
                x => x.DeleteByIdAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_Should_Return_False_When_Product_Not_Found()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _sut.DeleteProductAsync(999);

            // Assert
            result.Should().BeFalse();

            _productRepoMock.Verify(
                x => x.DeleteByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        // ============================================================
        // GetAllProductsAsync
        // ============================================================

        [Fact]
        public async Task GetAllProductsAsync_Should_Return_All_Products()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "A", SKU = "A-001", CategoryId = 1 },
            new Product { Id = 2, Name = "B", SKU = "B-001", CategoryId = 1 },
            new Product { Id = 3, Name = "C", SKU = "C-001", CategoryId = 1 }
        };

            _productRepoMock
                .Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetAllProductsAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(p => p.Name).Should().Contain(new[] { "A", "B", "C" });
        }

        [Fact]
        public async Task GetAllProductsAsync_Should_Return_Empty_When_No_Products()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetAllAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _sut.GetAllProductsAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ============================================================
        // IsSkuAvailableAsync
        // ============================================================

        [Fact]
        public async Task IsSkuAvailableAsync_Should_Return_True_When_SKU_Available()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.SkuExistsAsync("NEW-SKU", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.IsSkuAvailableAsync("NEW-SKU", default);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSkuAvailableAsync_Should_Return_False_When_SKU_Exists()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.SkuExistsAsync("EXIST-SKU", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.IsSkuAvailableAsync("EXIST-SKU", default);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsSkuAvailableAsync_Should_Exclude_ProductId_When_Provided()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.SkuExistsAsync("SKU-001", 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.IsSkuAvailableAsync("SKU-001", excludeProductId: 5);

            // Assert
            result.Should().BeTrue();
        }

        // ============================================================
        // GetLowStockProductsAsync
        // ============================================================

        [Fact]
        public async Task GetLowStockProductsAsync_Should_Return_Low_Stock_Products()
        {
            // Arrange
            var lowStockProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Low Stock A", SKU = "LS-A", StockQuantity = 2, MinimumStockThreshold = 5, CategoryId = 1 },
            new Product { Id = 2, Name = "Low Stock B", SKU = "LS-B", StockQuantity = 1, MinimumStockThreshold = 10, CategoryId = 1 }
        };

            _productRepoMock
                .Setup(x => x.GetLowStockProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(lowStockProducts);

            // Act
            var result = await _sut.GetLowStockProductsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(p => p.StockQuantity <= p.MinimumStockThreshold).Should().BeTrue();
        }

        [Fact]
        public async Task GetLowStockProductsAsync_Should_Return_Empty_When_None()
        {
            // Arrange
            _productRepoMock
                .Setup(x => x.GetLowStockProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            // Act
            var result = await _sut.GetLowStockProductsAsync();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
