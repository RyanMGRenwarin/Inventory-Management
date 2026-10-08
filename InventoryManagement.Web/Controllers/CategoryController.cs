using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.Filters;
using InventoryManagement.Web.ViewModels.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    /// <summary>
    /// Controller for transaction management.
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        /// <summary>
        /// Initializes a new instance of the CategoryController class.
        /// </summary>
        /// <param name="categoryService">Category service.</param>
        /// <param name="logger">Logger instance.</param>
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the category list page.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Category list view.</returns>
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index([FromQuery] CategoryFilterDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _categoryService.GetCategoriesAsync(filter, cancellationToken);

                var viewModel = new CategoryListViewModel
                {
                    Title = "Categories",
                    Categories = categories,
                    Filter = filter,
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories page");
                TempData["ErrorMessage"] = "An error occurred while loading categories.";
                return View("Error");
            }
        }

        /// <summary>
        /// Displays the category creation page.
        /// </summary>
        /// <returns>Category create view.</returns>
        [HttpGet]
        [Route("Create")]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Create()
        {
            var viewModel = new CategoryCreateViewModel
            {
                Title = "Create Category",
                IsAuthenticated = true,
                UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles category creation.
        /// </summary>
        /// <param name="viewModel">The category creation view model containing the category data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to category list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Create")]
        [AuthorizeRole(UserRole.Admin)]
        [ServiceFilter(typeof(ValidationActionFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var result = await _categoryService.CreateCategoryAsync(viewModel.Category, cancellationToken);
                TempData["SuccessMessage"] = $"Category '{result.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
                return View(viewModel);
            }
        }

        /// <summary>
        /// Displays the category edit page.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Category edit view.</returns>
        [HttpGet]
        [Route("Edit/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CategoryEditViewModel
                {
                    Title = "Edit Category",
                    Category = new CategoryCreateDto
                    {
                        Name = category.Name,
                        Description = category.Description
                    },
                    CategoryId = category.Id,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit category page for ID: {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles category update.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="viewModel">The category edit view model containing the updated category data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to category list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Edit/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ServiceFilter(typeof(ValidationActionFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryEditViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Mapping from ViewModel to DTO
                var updateDto = new CategoryCreateDto
                {
                    Name = viewModel.Category.Name,
                    Description = viewModel.Category.Description
                };

                var result = await _categoryService.UpdateCategoryAsync(id, updateDto, cancellationToken);
                TempData["SuccessMessage"] = $"Category '{result.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
                return View(viewModel);
            }
        }

        /// <summary>
        /// Handles category deletion.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to category list.</returns>
        [HttpPost]
        [Route("Delete/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
                if (result)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Category not found.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete category: {CategoryId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
