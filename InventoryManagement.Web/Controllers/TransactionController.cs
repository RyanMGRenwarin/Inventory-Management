using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.Filters;
using InventoryManagement.Web.ViewModels.Transaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Create(TransactionCreateDto createDto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var products = await _productService.GetAllProductsAsync(cancellationToken);
                    var warehouses = await _warehouseService.GetAvailableWarehousesAsync(cancellationToken);
                    var viewModel = new TransactionCreateViewModel
                    {
                        Title = "Create Transaction",
                        Transaction = createDto,
                        Products = products,
                        Warehouses = warehouses,
                        IsAuthenticated = true,
                        UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                    };
                    return View(viewModel);
                }

                // Get user ID from claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim?.Value, out int userId))
                {
                    throw new InvalidOperationException("User ID not found.");
                }

                var transaction = await _transactionService.CreateTransactionAsync(createDto, userId, cancellationToken);
                TempData["SuccessMessage"] = $"Transaction created successfully. {transaction.Quantity}x {transaction.ProductName} {transaction.TypeDisplayName}.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating transaction");
                ModelState.AddModelError(string.Empty, ex.Message);

                var products = await _productService.GetAllProductsAsync(cancellationToken);
                var warehouses = await _warehouseService.GetAvailableWarehousesAsync(cancellationToken);
                var viewModel = new TransactionCreateViewModel
                {
                    Title = "Create Transaction",
                    Transaction = createDto,
                    Products = products,
                    Warehouses = warehouses,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                TempData["ErrorMessage"] = "An error occurred while creating the transaction.";
                return RedirectToAction(nameof(Index));
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
    }
}
