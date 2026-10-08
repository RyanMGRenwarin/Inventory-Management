using FluentAssertions;
using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Web.Controllers;
using InventoryManagement.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Web.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<IWarehouseService> _warehouseServiceMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly HomeController _sut;

        public HomeControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _categoryServiceMock = new Mock<ICategoryService>();
            _warehouseServiceMock = new Mock<IWarehouseService>();
            _transactionServiceMock = new Mock<ITransactionService>();
            _loggerMock = new Mock<ILogger<HomeController>>();

            _sut = new HomeController(
                _productServiceMock.Object,
                _categoryServiceMock.Object,
                _warehouseServiceMock.Object,
                _transactionServiceMock.Object,
                _loggerMock.Object);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "admin"),
                new(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _sut.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());
        }

        // ============================================================
        // Index (GET) - Dashboard
        // ============================================================

        [Fact]
        public async Task Index_Should_Return_ViewResult_With_DashboardViewModel()
        {
            // Arrange
            _transactionServiceMock
                .Setup(x => x.GetTransactionSummaryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransactionSummaryDto { TotalTransactions = 10, TotalValue = 10000m });

            _productServiceMock
                .Setup(x => x.GetLowStockProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductResponseDto>());

            _productServiceMock
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductResponseDto>());

            _categoryServiceMock
                .Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CategoryResponseDto>());

            _warehouseServiceMock
                .Setup(x => x.GetAllWarehousesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseResponseDto>());

            // Act
            var result = await _sut.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<DashboardViewModel>().Subject;
            viewModel.TransactionSummary.TotalTransactions.Should().Be(10);
        }

        [Fact]
        public async Task Index_Should_Return_Error_View_When_Exception()
        {
            // Arrange
            _transactionServiceMock
                .Setup(x => x.GetTransactionSummaryAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await _sut.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("Error");
        }

        // ============================================================
        // Privacy (GET)
        // ============================================================

        [Fact]
        public void Privacy_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Privacy();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        // ============================================================
        // Error (GET)
        // ============================================================

        [Fact]
        public void Error_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Error();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
    }
}
