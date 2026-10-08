using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.Filters;
using InventoryManagement.Web.ViewModels.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace InventoryManagement.Web.Controllers
{
    /// <summary>
    /// Controller for transaction management.
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<TransactionController> _logger;

        /// <summary>
        /// Initializes a new instance of the TransactionController class.
        /// </summary>
        /// <param name="transactionService">Transaction service.</param>
        /// <param name="productService">Product service.</param>
        /// <param name="warehouseService">Warehouse service.</param>
        /// <param name="logger">Logger instance.</param>
        public TransactionController(
            ITransactionService transactionService,
            IProductService productService,
            IWarehouseService warehouseService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the transaction list page.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transaction list view.</returns>
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index([FromQuery] TransactionFilterDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsAsync(filter, cancellationToken);
                var products = await _productService.GetAllProductsAsync(cancellationToken);
                var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);

                var viewModel = new TransactionListViewModel
                {
                    Title = "Transactions",
                    Transactions = transactions,
                    Filter = filter,
                    Products = products,
                    Warehouses = warehouses,
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transactions page");
                TempData["ErrorMessage"] = "An error occurred while loading transactions.";
                return View("Error");
            }
        }

        /// <summary>
        /// Displays the transaction creation page.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transaction create view.</returns>
        [HttpGet]
        [Route("Create")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync(cancellationToken);
                var warehouses = await _warehouseService.GetAvailableWarehousesAsync(cancellationToken);

                var viewModel = new TransactionCreateViewModel
                {
                    Title = "Create Transaction",
                    Products = products,
                    Warehouses = warehouses,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create transaction page");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles transaction creation.
        /// </summary>
        /// <param name="createDto">Transaction creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to transaction list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "User,Admin")]
        [ServiceFilter(typeof(ValidationActionFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [FromForm] TransactionCreateViewModel viewModel, 
            CancellationToken cancellationToken = default)
        {
            // Remove default error binding
            ModelState.Remove("Transaction.UnitPrice");

            // Get the raw unit price from the form and assign it to the view model
            var unitPriceRaw = Request.Form["UnitPriceRaw"].ToString();
            viewModel.UnitPriceRaw = unitPriceRaw;

            // Validate the unit price
            if (string.IsNullOrWhiteSpace(unitPriceRaw))
            {
                ModelState.AddModelError("UnitPriceRaw", "Unit price is required");
            }
            else
            {
                viewModel.Transaction.UnitPrice = ParseDecimal(unitPriceRaw);

                if (viewModel.Transaction.UnitPrice < 0.01m || viewModel.Transaction.UnitPrice > 999999.99m)
                {
                    ModelState.AddModelError("UnitPriceRaw", "Unit price must be between 0.01 and 999,999.99");
                }
            }

            // Quantity Validation
            if (viewModel.Transaction.Quantity < 1)
            {
                ModelState.AddModelError("Transaction.Quantity", "Quantity must be at least 1");
            }

            if (!ModelState.IsValid)
            {
                // Reload dropdowns and return to view
                viewModel.Products = await _productService.GetAllProductsAsync(cancellationToken);
                viewModel.Warehouses = await _warehouseService.GetAvailableWarehousesAsync(cancellationToken);
                return View(viewModel);
            }

            try
            {
                // Get user ID from claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out int userId))
                {
                    throw new InvalidOperationException("User ID not found.");
                }

                var createDto = new TransactionCreateDto
                {
                    ProductId = viewModel.Transaction.ProductId,
                    WarehouseId = viewModel.Transaction.WarehouseId,
                    Type = viewModel.Transaction.Type,
                    Quantity = viewModel.Transaction.Quantity,
                    UnitPrice = viewModel.Transaction.UnitPrice,
                    Notes = viewModel.Transaction.Notes
                };

                var transaction = await _transactionService.CreateTransactionAsync(createDto, userId, cancellationToken);
                TempData["SuccessMessage"] = $"Transaction created successfully. {transaction.Quantity}x {transaction.ProductName} {transaction.TypeDisplayName}.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                viewModel.Products = await _productService.GetAllProductsAsync(cancellationToken);
                viewModel.Warehouses = await _warehouseService.GetAvailableWarehousesAsync(cancellationToken);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the transaction.");
                viewModel.Products = await _productService.GetAllProductsAsync(cancellationToken);
                viewModel.Warehouses = await _warehouseService.GetAvailableWarehousesAsync(cancellationToken);
                return View(viewModel);
            }
        }

        /// <summary>
        /// Handles transaction deletion (Admin only).
        /// </summary>
        /// <param name="id">Transaction ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to transaction list.</returns>
        [HttpPost]
        [Route("Delete/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _transactionService.DeleteTransactionAsync(id, cancellationToken);
                if (result)
                {
                    TempData["SuccessMessage"] = "Transaction deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction: {TransactionId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the transaction.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper Parser Decimal
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
