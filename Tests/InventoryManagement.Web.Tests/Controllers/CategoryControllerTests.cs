using FluentAssertions;
using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Web.Controllers;
using InventoryManagement.Web.ViewModels.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace InventoryManagement.Web.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly Mock<ILogger<CategoryController>> _loggerMock;
        private readonly CategoryController _sut;

        public CategoryControllerTests()
        {
            _categoryServiceMock = new Mock<ICategoryService>();
            _loggerMock = new Mock<ILogger<CategoryController>>();

            _sut = new CategoryController(_categoryServiceMock.Object, _loggerMock.Object);

            // Setup HttpContext with Admin user
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, "1"),
                new (ClaimTypes.Name, "admin"),
                new (ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var httpContext = new DefaultHttpContext { User = principal };

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
            var filter = new CategoryFilterDto();
            var categories = new List<CategoryResponseDto>
        {
            new CategoryResponseDto { Id = 1, Name = "Electronics", ProductCount = 5 }
        };

            _categoryServiceMock
                .Setup(x => x.GetCategoriesAsync(filter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Act
            var result = await _sut.Index(filter);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<CategoryListViewModel>().Subject;
            viewModel.Categories.Should().HaveCount(1);
        }

        [Fact]
        public async Task Index_Should_Return_Error_View_When_Exception()
        {
            // Arrange
            var filter = new CategoryFilterDto();

            _categoryServiceMock
                .Setup(x => x.GetCategoriesAsync(filter, It.IsAny<CancellationToken>()))
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
            viewResult.Model.Should().BeOfType<CategoryCreateViewModel>();
        }

        // ============================================================
        // Create (POST)
        // ============================================================

        [Fact]
        public async Task Create_Post_Should_Redirect_When_Success()
        {
            // Arrange
            var viewModel = new CategoryCreateViewModel
            {
                Category = new CategoryCreateDto { Name = "New", Description = "Desc" }
            };

            _sut.ModelState.Clear();

            _categoryServiceMock
                .Setup(x => x.CreateCategoryAsync(It.IsAny<CategoryCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CategoryResponseDto { Id = 1, Name = "New" });

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
            var viewModel = new CategoryCreateViewModel
            {
                Category = new CategoryCreateDto { Name = "" }
            };
            _sut.ModelState.AddModelError("Category.Name", "Required");

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_Post_Should_Return_View_When_Duplicate_Name()
        {
            // Arrange
            var viewModel = new CategoryCreateViewModel
            {
                Category = new CategoryCreateDto { Name = "Duplicate" }
            };

            _categoryServiceMock
                .Setup(x => x.CreateCategoryAsync(It.IsAny<CategoryCreateDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Category name 'Duplicate' already exists."));

            // Act
            var result = await _sut.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
        }

        // ============================================================
        // Edit (GET)
        // ============================================================

        [Fact]
        public async Task Edit_Get_Should_Return_ViewResult_When_Found()
        {
            // Arrange
            var category = new CategoryResponseDto { Id = 1, Name = "Electronics", Description = "Desc" };

            _categoryServiceMock
                .Setup(x => x.GetCategoryByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.Edit(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var viewModel = viewResult.Model.Should().BeOfType<CategoryEditViewModel>().Subject;
            viewModel.CategoryId.Should().Be(1);
        }

        [Fact]
        public async Task Edit_Get_Should_Redirect_When_Not_Found()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.GetCategoryByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CategoryResponseDto?)null);

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
            var viewModel = new CategoryEditViewModel
            {
                CategoryId = 1,
                Category = new CategoryCreateDto { Name = "Updated", Description = "New" }
            };

            _categoryServiceMock
                .Setup(x => x.UpdateCategoryAsync(1, It.IsAny<CategoryCreateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CategoryResponseDto { Id = 1, Name = "Updated" });

            // Act
            var result = await _sut.Edit(1, viewModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Post_Should_Return_View_When_Duplicate()
        {
            // Arrange
            var viewModel = new CategoryEditViewModel
            {
                CategoryId = 1,
                Category = new CategoryCreateDto { Name = "Duplicate" }
            };

            _categoryServiceMock
                .Setup(x => x.UpdateCategoryAsync(1, It.IsAny<CategoryCreateDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Category name 'Duplicate' already exists."));

            // Act
            var result = await _sut.Edit(1, viewModel);

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
            _categoryServiceMock
                .Setup(x => x.DeleteCategoryAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.Delete(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");
            _sut.TempData["SuccessMessage"].Should().Be("Category deleted successfully.");
        }

        [Fact]
        public async Task Delete_Should_Show_Error_When_Not_Found()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.DeleteCategoryAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.Delete(999);

            // Assert
            _sut.TempData["ErrorMessage"].Should().Be("Category not found.");
        }

        [Fact]
        public async Task Delete_Should_Show_Error_When_Has_Products()
        {
            // Arrange
            _categoryServiceMock
                .Setup(x => x.DeleteCategoryAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Cannot delete category with existing products."));

            // Act
            var result = await _sut.Delete(1);

            // Assert
            _sut.TempData["ErrorMessage"].Should().Be("Cannot delete category with existing products.");
        }
    }
}
