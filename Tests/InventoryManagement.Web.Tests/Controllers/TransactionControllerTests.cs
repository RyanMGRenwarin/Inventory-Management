using FluentAssertions;
using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.Controllers;
using InventoryManagement.Web.ViewModels.Transaction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Web.Tests.Controllers
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IWarehouseService> _warehouseServiceMock;
        private readonly Mock<ILogger<TransactionController>> _loggerMock;
        private readonly TransactionController _sut;

        public TransactionControllerTests()
        {
            _transactionServiceMock = new Mock<ITransactionService>();
            _productServiceMock = new Mock<IProductService>();
            _warehouseServiceMock = new Mock<IWarehouseService>();
            _loggerMock = new Mock<ILogger<TransactionController>>();

            _sut = new TransactionController(
                _transactionServiceMock.Object,
                _productServiceMock.Object,
                _warehouseServiceMock.Object,
                _loggerMock.Object);

            // Setup HttpContext with Admin user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "admin"),
                new(ClaimTypes.Role, "Admin")
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
            var filter = new TransactionFilterDto();
            var transactions = new TransactionListResponseDto
            {
                Transactions =
                [
                    new() { Id = 1, ProductName = "P1", WarehouseName = "W1", Quantity = 10 }
                ],
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _transactionServiceMock
                .Setup(x => x.GetTransactionsAsync(filter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            _productServiceMock
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductResponseDto>());

            _warehouseServiceMock
                .Setup(x => x.GetAllWarehousesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseResponseDto>());

            // Act
            var result = await _sut.Index(filter);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<TransactionListViewModel>().Subject;
            viewModel.Transactions.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Index_Should_Return_Error_View_When_Exception()
        {
            // Arrange
            var filter = new TransactionFilterDto();

            _transactionServiceMock
                .Setup(x => x.GetTransactionsAsync(filter, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB Error"));

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
        public async Task Create_Get_Should_Return_ViewResult()
        {
            // Arrange
            _productServiceMock
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductResponseDto>());

            _warehouseServiceMock
                .Setup(x => x.GetAvailableWarehousesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseResponseDto>());

            // Act
            var result = await _sut.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<TransactionCreateViewModel>();
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Create_Post_Should_Redirect_When_Success()
        {
            // Arrange
            var viewModel = new TransactionCreateViewModel
            {
                Transaction = new TransactionCreateDto
                {
                    ProductId = 1,
                    WarehouseId = 1,
                    Type = TransactionType.Inbound,
                    Quantity = 10,
                    UnitPrice = 100
                }
            };

            _transactionServiceMock
                .Setup(x => x.CreateTransactionAsync(It.IsAny<TransactionCreateDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransactionResponseDto { Id = 1, ProductName = "P1" });

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["UnitPriceRaw"] = "100"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_ModelState_Invalid()
        {
            // Arrange
            var viewModel = new TransactionCreateViewModel
            {
                Transaction = new TransactionCreateDto()
            };
            _sut.ModelState.AddModelError("Transaction.Quantity", "Required");

            _productServiceMock
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductResponseDto>());

            _warehouseServiceMock
                .Setup(x => x.GetAvailableWarehousesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseResponseDto>());

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["UnitPriceRaw"] = "100"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_Service_Throws()
        {
            // Arrange
            var viewModel = new TransactionCreateViewModel
            {
                Transaction = new TransactionCreateDto
                {
                    ProductId = 1,
                    WarehouseId = 1,
                    Type = TransactionType.Inbound,
                    Quantity = 10,
                    UnitPrice = 100
                }
            };

            _transactionServiceMock
                .Setup(x => x.CreateTransactionAsync(It.IsAny<TransactionCreateDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Insufficient stock"));

            _productServiceMock
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductResponseDto>());

            _warehouseServiceMock
                .Setup(x => x.GetAvailableWarehousesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseResponseDto>());

            var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["UnitPriceRaw"] = "100"
            };
            _sut.ControllerContext.HttpContext.Request.Form = new FormCollection(formData);

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ============================================================
        // Delete (POST)
        // ============================================================

        [Fact]
        public async Task Delete_Should_Redirect_When_Success()
        {
            // Arrange
            _transactionServiceMock
                .Setup(x => x.DeleteTransactionAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _sut.TempData["SuccessMessage"].Should().Be("Transaction deleted successfully.");
        }

        [Fact]
        public async Task Delete_Should_Show_Error_When_Not_Found()
        {
            // Arrange
            _transactionServiceMock
                .Setup(x => x.DeleteTransactionAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Delete(999);

            // Assert
            _sut.TempData["ErrorMessage"].Should().Be("Transaction not found.");
        }
    }
}
