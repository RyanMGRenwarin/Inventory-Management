using FluentAssertions;
using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Web.Controllers;
using InventoryManagement.Web.ViewModels.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Web.Tests.Controllers
{
    public class WarehouseControllerTests
    {
        private readonly Mock<IWarehouseService> _warehouseServiceMock;
        private readonly Mock<ILogger<WarehouseController>> _loggerMock;
        private readonly WarehouseController _sut;

        public WarehouseControllerTests()
        {
            _warehouseServiceMock = new Mock<IWarehouseService>();
            _loggerMock = new Mock<ILogger<WarehouseController>>();

            _sut = new WarehouseController(_warehouseServiceMock.Object, _loggerMock.Object);

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
            var filter = new WarehouseFilterDto();
            var warehouses = new List<WarehouseResponseDto>
        {
            new WarehouseResponseDto { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000 }
        };

            _warehouseServiceMock
                .Setup(x => x.GetWarehousesAsync(filter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouses);

            // Act
            var result = await _sut.Index(filter);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<WarehouseListViewModel>().Subject;
            viewModel.Warehouses.Should().HaveCount(1);
        }

        [Fact]
        public async Task Index_Should_Return_Error_View_When_Exception()
        {
            // Arrange
            var filter = new WarehouseFilterDto();

            _warehouseServiceMock
                .Setup(x => x.GetWarehousesAsync(filter, It.IsAny<CancellationToken>()))
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
        public void Create_Get_Should_Return_ViewResult()
        {
            // Act
            var result = _sut.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<WarehouseCreateViewModel>();
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Create_Post_Should_Redirect_When_Success()
        {
            // Arrange
            var viewModel = new WarehouseCreateViewModel
            {
                Warehouse = new WarehouseCreateDto { Name = "New", Location = "Surabaya", Capacity = 500 }
            };

            _warehouseServiceMock
                .Setup(x => x.CreateWarehouseAsync(It.IsAny<WarehouseCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WarehouseResponseDto { Id = 1, Name = "New" });

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _sut.TempData["SuccessMessage"].Should().NotBeNull();
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_ModelState_Invalid()
        {
            // Arrange
            var viewModel = new WarehouseCreateViewModel
            {
                Warehouse = new WarehouseCreateDto { Name = "", Location = "", Capacity = 0 }
            };
            _sut.ModelState.AddModelError("Warehouse.Name", "Required");

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<WarehouseCreateViewModel>();
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_Exception()
        {
            // Arrange
            var viewModel = new WarehouseCreateViewModel
            {
                Warehouse = new WarehouseCreateDto { Name = "New", Location = "Surabaya", Capacity = 500 }
            };

            _warehouseServiceMock
                .Setup(x => x.CreateWarehouseAsync(It.IsAny<WarehouseCreateDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _sut.TempData["ErrorMessage"].Should().NotBeNull();
        }

        // ============================================================
        // Edit (GET)
        // ============================================================

        [Fact]
        public async Task Edit_Get_Should_Return_ViewResult_When_Found()
        {
            // Arrange
            var warehouse = new WarehouseResponseDto { Id = 1, Name = "Main", Location = "Jakarta", Capacity = 1000 };

            _warehouseServiceMock
                .Setup(x => x.GetWarehouseByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(warehouse);

            // Act
            var result = await _sut.Edit(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<WarehouseEditViewModel>().Subject;
            viewModel.WarehouseId.Should().Be(1);
        }

        [Fact]
        public async Task Edit_Get_Should_Redirect_When_Not_Found()
        {
            // Arrange
            _warehouseServiceMock
                .Setup(x => x.GetWarehouseByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((WarehouseResponseDto?)null);

            // Act
            var result = await _sut.Edit(999);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        // ============================================================
        // Edit (POST)
        // ============================================================

        [Fact]
        public async Task Edit_Post_Should_Redirect_When_Success()
        {
            // Arrange
            var viewModel = new WarehouseEditViewModel
            {
                WarehouseId = 1,
                Warehouse = new WarehouseCreateDto { Name = "Updated", Location = "Bandung", Capacity = 2000 }
            };

            _warehouseServiceMock
                .Setup(x => x.UpdateWarehouseAsync(1, It.IsAny<WarehouseCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new WarehouseResponseDto { Id = 1, Name = "Updated" });

            // Act
            var result = await _sut.Edit(1, viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Post_Should_Return_View_When_ModelState_Invalid()
        {
            // Arrange
            var viewModel = new WarehouseEditViewModel
            {
                WarehouseId = 1,
                Warehouse = new WarehouseCreateDto()
            };
            _sut.ModelState.AddModelError("Warehouse.Name", "Required");

            // Act
            var result = await _sut.Edit(1, viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        // ============================================================
        // Delete (POST)
        // ============================================================

        [Fact]
        public async Task Delete_Should_Redirect_When_Success()
        {
            // Arrange
            _warehouseServiceMock
                .Setup(x => x.DeleteWarehouseAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _sut.TempData["SuccessMessage"].Should().Be("Warehouse deleted successfully.");
        }

        [Fact]
        public async Task Delete_Should_Show_Error_When_Not_Found()
        {
            // Arrange
            _warehouseServiceMock
                .Setup(x => x.DeleteWarehouseAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Delete(999);

            // Assert
            _sut.TempData["ErrorMessage"].Should().Be("Warehouse not found.");
        }

        [Fact]
        public async Task Delete_Should_Show_Error_When_Has_Transactions()
        {
            // Arrange
            _warehouseServiceMock
                .Setup(x => x.DeleteWarehouseAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot delete warehouse with existing transactions."));

            // Act
            var result = await _sut.Delete(1);

            // Assert
            _sut.TempData["ErrorMessage"].Should().Be("Cannot delete warehouse with existing transactions.");
        }
    }
}
