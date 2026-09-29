using InventoryManagement.Application.DTOs.Warehouse;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.Filters;
using InventoryManagement.Web.ViewModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Web.Controllers
{
    /// <summary>
    /// Controller for warehouse management.
    /// </summary>
    [Authorize]
    [Route("[controller]")]
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseController> _logger;

        /// <summary>
        /// Initializes a new instance of the WarehouseController class.
        /// </summary>
        /// <param name="warehouseService">Warehouse service.</param>
        /// <param name="logger">Logger instance.</param>
        public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Displays the warehouse list page.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Warehouse list view.</returns>
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index([FromQuery] WarehouseFilterDto filter, CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouses = await _warehouseService.GetWarehousesAsync(filter, cancellationToken);

                var viewModel = new WarehouseListViewModel
                {
                    Title = "Warehouses",
                    Warehouses = warehouses,
                    Filter = filter,
                    IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warehouses page");
                TempData["ErrorMessage"] = "An error occurred while loading warehouses.";
                return View("Error");
            }
        }

        /// <summary>
        /// Displays the warehouse creation page.
        /// </summary>
        /// <returns>Warehouse create view.</returns>
        [HttpGet]
        [Route("Create")]
        [AuthorizeRole(UserRole.Admin)]
        public IActionResult Create()
        {
            var viewModel = new WarehouseCreateViewModel
            {
                Title = "Create Warehouse",
                IsAuthenticated = true,
                UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles warehouse creation.
        /// </summary>
        /// <param name="viewModel">The warehouse creation view model containing the warehouse data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to warehouse list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Create")]
        [AuthorizeRole(UserRole.Admin)]
        [ServiceFilter(typeof(ValidationActionFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseCreateViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var createDto = new WarehouseCreateDto
                {
                    Name = viewModel.Warehouse.Name,
                    Location = viewModel.Warehouse.Location,
                    Capacity = viewModel.Warehouse.Capacity
                };

                var warehouse = await _warehouseService.CreateWarehouseAsync(createDto, cancellationToken);
                TempData["SuccessMessage"] = $"Warehouse '{warehouse.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating warehouse");
                TempData["ErrorMessage"] = "An error occurred while creating the warehouse.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the warehouse edit page.
        /// </summary>
        /// <param name="id">Warehouse ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Warehouse edit view.</returns>
        [HttpGet]
        [Route("Edit/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, cancellationToken);
                if (warehouse == null)
                {
                    TempData["ErrorMessage"] = "Warehouse not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new WarehouseEditViewModel
                {
                    Title = "Edit Warehouse",
                    Warehouse = new WarehouseCreateDto
                    {
                        Name = warehouse.Name,
                        Location = warehouse.Location,
                        Capacity = warehouse.Capacity
                    },
                    WarehouseId = warehouse.Id,
                    IsAuthenticated = true,
                    UserRole = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit warehouse page for ID: {WarehouseId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles warehouse update.
        /// </summary>
        /// <param name="id">Warehouse ID.</param>
        /// <param name="viewModel">The warehouse edit view model containing the updated warehouse data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to warehouse list on success, or returns to form on error.</returns>
        [HttpPost]
        [Route("Edit/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ServiceFilter(typeof(ValidationActionFilter))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarehouseEditViewModel viewModel, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var updateDto = new WarehouseCreateDto
                {
                    Name = viewModel.Warehouse.Name,
                    Location = viewModel.Warehouse.Location,
                    Capacity = viewModel.Warehouse.Capacity
                };

                var warehouse = await _warehouseService.UpdateWarehouseAsync(id, updateDto, cancellationToken);
                TempData["SuccessMessage"] = $"Warehouse '{warehouse.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the warehouse.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles warehouse deletion.
        /// </summary>
        /// <param name="id">Warehouse ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Redirect to warehouse list.</returns>
        [HttpPost]
        [Route("Delete/{id}")]
        [AuthorizeRole(UserRole.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _warehouseService.DeleteWarehouseAsync(id, cancellationToken);
                if (result)
                {
                    TempData["SuccessMessage"] = "Warehouse deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Warehouse not found.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete warehouse: {WarehouseId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the warehouse.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
