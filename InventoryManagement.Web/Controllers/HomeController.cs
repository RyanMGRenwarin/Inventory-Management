using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    /// <summary>
    /// Home controller for the main application pages.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWarehouseService _warehouseService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the HomeController class.
        /// </summary>
        /// <param name="productService">Product service.</param>
        /// <param name="categoryService">Category service.</param>
        /// <param name="warehouseService">Warehouse service.</param>
        /// <param name="transactionService">Transaction service.</param>
        /// <param name="logger">Logger instance.</param>
        public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IWarehouseService warehouseService,
        ITransactionService transactionService,
        ILogger<HomeController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the dashboard page.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dashboard view.</returns>
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = await _transactionService.GetTransactionSummaryAsync(cancellationToken);
                var lowStockProducts = await _productService.GetLowStockProductsAsync(cancellationToken);
                var categories = await _categoryService.GetAllCategoriesAsync(cancellationToken);
                var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);

                var viewModel = new DashboardViewModel
                {
                    Title = "Dashboard",
                    TransactionSummary = summary,
                    LowStockProducts = lowStockProducts,
                    TotalProducts = await _productService.GetAllProductsAsync(cancellationToken).ContinueWith(t => t.Result.Count()),
                    TotalCategories = categories.Count(),
                    TotalWarehouses = warehouses.Count(),
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value,
                    UserFullName = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View("Error");
            }
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>Privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page.
        /// </summary>
        /// <returns>Error view.</returns>
        public IActionResult Error()
        {
            return View();
        }
    }
}
