using FluentAssertions;
using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Web.Controllers;
using InventoryManagement.Web.ViewModels.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Web.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<ILogger<ProductController>> _loggerMock;
        private readonly ProductController _sut;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _loggerMock = new Mock<ILogger<ProductController>>();

            _sut = new ProductController(
                _productServiceMock.Object,
                _categoryServiceMock.Object,
                _loggerMock.Object);

            // Setup HttpContext with authenticated admin user
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, "1"),
                new (ClaimTypes.Name, "admin"),
                new (ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _sut.TempData = new TempDataDictionary(
                        httpContext,
                        Mock.Of<ITempDataProvider>());
        }

        // ============================================================
        // Index (GET)
        // ============================================================

        [Fact]
        public async Task Index_Should_Return_ViewResult_With_ViewModel()
        {
            // Arrange
            var filter = new ProductFilterDto();
            var productList = new ProductListResponseDto
            {
                Products = new List<ProductResponseDto>
            {
                new ProductResponseDto { Id = 1, Name = "Test", SKU = "TEST-001" }
            },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _productServiceMock
                .Setup(x => x.GetProductsAsync(filter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productList);

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            // Act
            var result = await _sut.Index(filter);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<ProductListViewModel>().Subject;
            viewModel.Products.TotalCount.Should().Be(1);
            viewModel.Title.Should().Be("Products");
        }

        [Fact]
        public async Task Index_Should_Return_Error_View_When_Exception_Thrown()
        {
            // Arrange
            var filter = new ProductFilterDto();

            _productServiceMock
                .Setup(x => x.GetProductsAsync(filter, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.Index(filter);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Error");
        }

        // ============================================================
        // Create (GET)
        // ============================================================

        [Fact]
        public async Task Create_Get_Should_Return_ViewResult_With_ViewModel()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>
                {
                new CategoryResponseDto { Id = 1, Name = "Category A" }
                });

            // Act
            var result = await _sut.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<ProductCreateViewModel>().Subject;
            viewModel.Categories.Should().HaveCount(1);
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Create_Post_Should_Redirect_To_Index_When_Success()
        {
            // Arrange
            var viewModel = new ProductCreateViewModel
            {
                Product = new ProductCreateDto
                {
                    Name = "Test",
                    SKU = "TEST-001",
                    Price = 100m,
                    StockQuantity = 10,
                    MinimumStockThreshold = 5,
                    CategoryId = 1
                },
                PriceRaw = "100"
            };

            _sut.ModelState.Clear();

            // Simulate form data
            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "100"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _productServiceMock
                .Setup(x => x.CreateProductAsync(It.IsAny<ProductCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductResponseDto { Id = 1, Name = "Test", SKU = "TEST-001" });

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_Price_Empty()
        {
            // Arrange
            var viewModel = new ProductCreateViewModel
            {
                Product = new ProductCreateDto { Name = "Test", SKU = "TEST-001", CategoryId = 1 }
            };

            // Empty PriceRaw
            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = ""
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<ProductCreateViewModel>();
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_Price_Out_Of_Range()
        {
            // Arrange
            var viewModel = new ProductCreateViewModel
            {
                Product = new ProductCreateDto { Name = "Test", SKU = "TEST-001", CategoryId = 1 }
            };

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "1000000" // > 999,999.99
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Create_Post_Should_Accept_Comma_Decimal_Separator()
        {
            // Arrange
            var viewModel = new ProductCreateViewModel
            {
                Product = new ProductCreateDto
                {
                    Name = "Test",
                    SKU = "TEST-001",
                    StockQuantity = 10,
                    MinimumStockThreshold = 5,
                    CategoryId = 1
                }
            };

            _sut.ModelState.Clear();

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "99,99" // Comma separator
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _productServiceMock
                .Setup(x => x.CreateProductAsync(It.IsAny<ProductCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductResponseDto { Id = 1, Name = "Test", SKU = "TEST-001" });

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _productServiceMock.Verify(
                x => x.CreateProductAsync(
                    It.Is<ProductCreateDto>(dto => dto.Price == 99.99m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_Duplicate_SKU()
        {
            // Arrange
            var viewModel = new ProductCreateViewModel
            {
                Product = new ProductCreateDto { Name = "Test", SKU = "DUP-001", Price = 100m, CategoryId = 1 }
            };

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "100"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _productServiceMock
                .Setup(x => x.CreateProductAsync(It.IsAny<ProductCreateDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("SKU already exists"));

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
            _sut.ModelState[string.Empty]!.Errors.Should()
                .Contain(e => e.ErrorMessage.Contains("SKU already exists"));
        }

        // ============================================================
        // Edit (GET)
        // ============================================================

        [Fact]
        public async Task Edit_Get_Should_Return_ViewResult_When_Product_Found()
        {
            // Arrange
            var product = new ProductResponseDto
            {
                Id = 1,
                Name = "Test",
                SKU = "TEST-001",
                Price = 100m,
                StockQuantity = 10,
                MinimumStockThreshold = 5,
                CategoryId = 1
            };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            // Act
            var result = await _sut.Edit(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<ProductEditViewModel>().Subject;
            viewModel.Product.Id.Should().Be(1);
            viewModel.PriceRaw.Should().Be("100.00");
        }

        [Fact]
        public async Task Edit_Get_Should_Redirect_When_Product_Not_Found()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductResponseDto?)null);

            // Act
            var result = await _sut.Edit(999);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        // ============================================================
        // Edit (POST)
        // ============================================================

        [Fact]
        public async Task Edit_Post_Should_Redirect_To_Index_When_Success()
        {
            // Arrange
            var viewModel = new ProductEditViewModel
            {
                Product = new ProductUpdateDto
                {
                    Id = 1,
                    Name = "Updated",
                    SKU = "TEST-001",
                    Price = 150m,
                    StockQuantity = 20,
                    MinimumStockThreshold = 5,
                    CategoryId = 1
                },
                PriceRaw = "150"
            };

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "150"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _productServiceMock
                .Setup(x => x.UpdateProductAsync(It.IsAny<ProductUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductResponseDto { Id = 1, Name = "Updated", SKU = "TEST-001" });

            // Act
            var result = await _sut.Edit(1, viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Post_Should_Return_NotFound_When_Id_Mismatch()
        {
            // Arrange
            var viewModel = new ProductEditViewModel
            {
                Product = new ProductUpdateDto { Id = 2, Name = "Test", SKU = "TEST-001", CategoryId = 1 }
            };

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "100"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            // Act
            var result = await _sut.Edit(1, viewModel); // id=1, viewModel.Product.Id=2

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Edit_Post_Should_Return_View_When_Price_Empty()
        {
            // Arrange
            var viewModel = new ProductEditViewModel
            {
                Product = new ProductUpdateDto { Id = 1, Name = "Test", SKU = "TEST-001", CategoryId = 1 }
            };

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = ""
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            // Act
            var result = await _sut.Edit(1, viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Edit_Post_Should_Accept_Comma_Separator()
        {
            // Arrange
            var viewModel = new ProductEditViewModel
            {
                Product = new ProductUpdateDto
                {
                    Id = 1,
                    Name = "Updated",
                    SKU = "TEST-001",
                    StockQuantity = 20,
                    MinimumStockThreshold = 5,
                    CategoryId = 1
                }
            };

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["PriceRaw"] = "150,75"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            _productServiceMock
                .Setup(x => x.UpdateProductAsync(It.IsAny<ProductUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductResponseDto { Id = 1, Name = "Updated", SKU = "TEST-001" });

            // Act
            var result = await _sut.Edit(1, viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _productServiceMock.Verify(
                x => x.UpdateProductAsync(
                    It.Is<ProductUpdateDto>(dto => dto.Price == 150.75m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ============================================================
        // Delete (GET)
        // ============================================================

        [Fact]
        public async Task Delete_Get_Should_Return_ViewResult_When_Product_Found()
        {
            // Arrange
            var product = new ProductResponseDto { Id = 1, Name = "Test", SKU = "TEST-001" };

            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.Delete(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<ProductDeleteViewModel>().Subject;
            viewModel.Product.Id.Should().Be(1);
        }

        [Fact]
        public async Task Delete_Get_Should_Redirect_When_Product_Not_Found()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductResponseDto?)null);

            // Act
            var result = await _sut.Delete(999);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        // ============================================================
        // DeleteConfirmed (POST)
        // ============================================================

        [Fact]
        public async Task DeleteConfirmed_Should_Redirect_With_Success_Message()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.DeleteProductAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteConfirmed(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _sut.TempData["SuccessMessage"].Should().Be("Product deleted successfully.");
        }

        [Fact]
        public async Task DeleteConfirmed_Should_Show_Error_When_Not_Found()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.DeleteProductAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteConfirmed(999);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            _sut.TempData["ErrorMessage"].Should().Be("Product not found.");
        }

        // ============================================================
        // CheckSku (GET - AJAX)
        // ============================================================

        [Fact]
        public async Task CheckSku_Should_Return_Json_Available_True()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.IsSkuAvailableAsync("NEW-SKU", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.CheckSku("NEW-SKU", null);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckSku_Should_Return_Json_Available_False()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.IsSkuAvailableAsync("EXIST-SKU", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.CheckSku("EXIST-SKU", null);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckSku_Should_Return_Json_With_Error_When_Exception()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.IsSkuAvailableAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _sut.CheckSku("TEST-SKU", null);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().NotBeNull();
        }
    }
}
