using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.Filters;
using InventoryManagement.Web.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace InventoryManagement.Web.Controllers
{
    /// <summary>
    /// Controller for product management.
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// Initializes a new instance of the ProductController class.
        /// </summary>
        /// <param name="productService">Product service.</param>
        /// <param name="categoryService">Category service.</param>
        /// <param name="logger">Logger instance.</param>
        public ProductController(IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the product list page.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Product list view.</returns>
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter, CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productService.GetProductsAsync(filter, cancellationToken);
                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);

                var viewModel = new ProductListViewModel
                {
                    Title = "Products",
                    Products = products,
                    Filter = filter,
                    Categories = categories,
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products page");
                TempData["ErrorMessage"] = "An error occurred while loading products.";
                return View("Error");
            }
        }

        /// <summary>
        /// Displays the product creation page.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Product create view.</returns>
        [HttpGet]
        [Route("Create")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);

                var viewModel = new ProductCreateViewModel
                {
                    Title = "Create Product",
                    Categories = categories,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create product page");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles product creation.
        /// </summary>
        /// <param name="viewModel">The product creation view model containing the product data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to product list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Create")]
        [AuthorizeRole(UserRole.Admin)]
        [ServiceFilter(typeof(ValidationActionFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel, CancellationToken cancellationToken = default)
        {
            // Remove error binding default
            ModelState.Remove("Product.Price");

            // Retrieve value from PriceRaw
            var priceRaw = Request.Form["PriceRaw"].ToString();
            //viewModel.PriceRaw = priceRaw;

            // Validate: Check if Price is empty or Price is not in valid range (0.01 to 999,999.99)
            if (string.IsNullOrWhiteSpace(priceRaw))
            {
                ModelState.AddModelError("PriceRaw", "Price is required");
            }
            else
            {
                viewModel.Product.Price = ParseDecimal(priceRaw);

                if (viewModel.Product.Price < 0.01m || viewModel.Product.Price > 999999.99m)
                {
                    ModelState.AddModelError("PriceRaw", "Price must be between 0.01 and 999,999.99");
                }
            }

            if (!ModelState.IsValid)
            {
                

                // Reload categories
                viewModel.Categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);

                viewModel.PriceRaw = priceRaw;
                return View(viewModel);
            }

            try
            {
                // Mapping to DTO
                var createDto = new ProductCreateDto
                {
                    Name = viewModel.Product.Name,
                    SKU = viewModel.Product.SKU,
                    Description = viewModel.Product.Description,
                    Price = viewModel.Product.Price,
                    StockQuantity = viewModel.Product.StockQuantity,
                    MinimumStockThreshold = viewModel.Product.MinimumStockThreshold,
                    CategoryId = viewModel.Product.CategoryId
                };

                var product = await _productService.CreateProductAsync(createDto, cancellationToken);
                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                viewModel.Categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
                viewModel.PriceRaw = priceRaw;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the product.");
                viewModel.Categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
                viewModel.PriceRaw = priceRaw;
                return View(viewModel);
            }
        }

        /// <summary>
        /// Displays the product edit page.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Product edit view.</returns>
        [HttpGet]
        [Route("Edit/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id, cancellationToken);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);

                var viewModel = new ProductEditViewModel
                {
                    Title = "Edit Product",
                    Product = new ProductUpdateDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        SKU = product.SKU,
                        Description = product.Description,
                        Price = product.Price, /*Save Price as decimal for validation,
                                                 but will use PriceRaw for input/output */
                        StockQuantity = product.StockQuantity,
                        MinimumStockThreshold = product.MinimumStockThreshold,
                        CategoryId = product.CategoryId
                    },
                    // Fill PriceRaw with format that can be parsed (dot)
                    PriceRaw = product.Price.ToString("F2", CultureInfo.InvariantCulture),
                    Categories = categories,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit product page for ID: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles product update.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="viewModel">The product edit view model containing the updated product data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to product list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Edit/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [FromForm] ProductEditViewModel viewModel,
            CancellationToken cancellationToken = default)
        {
            // Remove error binding default for Price
            ModelState.Remove("Product.Price");

            // Retrieve value from PriceRaw (string)
            var priceRaw = Request.Form["PriceRaw"].ToString();

            // Validate: Check if Price is empty or Price is not in valid range (0.01 to 999,999.99)
            if (string.IsNullOrWhiteSpace(priceRaw))
            {
                ModelState.AddModelError("PriceRaw", "Price is required");
            }
            else
            {
                viewModel.Product.Price = ParseDecimal(priceRaw);

                if (viewModel.Product.Price < 0.01m || viewModel.Product.Price > 999999.99m)
                {
                    ModelState.AddModelError("PriceRaw", "Price must be between 0.01 and 999,999.99");
                }
            }

            if (id != viewModel.Product.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                viewModel.Categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
                // Return PriceRaw that has been entered by user to the form
                viewModel.PriceRaw = priceRaw;
                return View(viewModel);
            }

            try
            {
                var updateDto = new ProductUpdateDto
                {
                    Id = viewModel.Product.Id,
                    Name = viewModel.Product.Name,
                    SKU = viewModel.Product.SKU,
                    Description = viewModel.Product.Description,
                    Price = viewModel.Product.Price,
                    StockQuantity = viewModel.Product.StockQuantity,
                    MinimumStockThreshold = viewModel.Product.MinimumStockThreshold,
                    CategoryId = viewModel.Product.CategoryId
                };

                var result = await _productService.UpdateProductAsync(updateDto, cancellationToken);
                TempData["SuccessMessage"] = $"Product '{result.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                viewModel.Categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
                viewModel.PriceRaw = priceRaw; // Return PriceRaw that has been entered by user
                return View(viewModel);
            }
        }

        /// <summary>
        /// Displays the product delete confirmation page.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Product delete view.</returns>
        [HttpGet]
        [Route("Delete/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id, cancellationToken);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new ProductDeleteViewModel
                {
                    Title = "Delete Product",
                    Product = product,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete product page for ID: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles product deletion.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to product list.</returns>
        [HttpPost]
        [Route("Delete/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id, cancellationToken);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Product not found.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Checks if a SKU is available (for AJAX validation).
        /// </summary>
        /// <param name="sku">SKU to check.</param>
        /// <param name="id">Optional product ID to exclude.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>JSON result indicating availability.</returns>
        [HttpGet]
        [Route("CheckSku")]
        public async Task<IActionResult> CheckSku([FromQuery] string sku, [FromQuery] int? id, CancellationToken cancellationToken = default)
        {
            try
            {
                var isAvailable = await _productService.IsSkuAvailableAsync(sku, id, cancellationToken);
                return Json(new { available = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SKU availability: {SKU}", sku);
                return Json(new { available = false, error = "An error occurred" });
            }
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            var normalized = cleaned.Replace(".", ",");
            if (decimal.TryParse(normalized, NumberStyles.Any, new CultureInfo("id-ID"), out var result))
                return result;
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;
            return 0;
        }
    }
}
