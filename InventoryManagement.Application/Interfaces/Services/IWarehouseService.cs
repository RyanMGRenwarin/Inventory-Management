using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for warehouse management.
    /// </summary>
    public interface IWarehouseService
    {
        /// <summary>
        /// Gets a paginated list of warehouses based on filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated warehouse list.</returns>
        Task<IEnumerable<WarehouseResponseDto>> GetWarehousesAsync(WarehouseFilterDto filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a warehouse by its ID.
        /// </summary>
        /// <param name="id">Warehouse ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Warehouse details if found, otherwise null.</returns>
        Task<WarehouseResponseDto?> GetWarehouseByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new warehouse.
        /// </summary>
        /// <param name="createDto">Warehouse creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created warehouse.</returns>
        Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseCreateDto createDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing warehouse.
        /// </summary>
        /// <param name="id">Warehouse ID.</param>
        /// <param name="updateDto">Warehouse update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated warehouse.</returns>
        Task<WarehouseResponseDto> UpdateWarehouseAsync(int id, WarehouseCreateDto updateDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a warehouse by its ID (soft delete).
        /// </summary>
        /// <param name="id">Warehouse ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, otherwise false.</returns>
        Task<bool> DeleteWarehouseAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all warehouses for dropdown/selection purposes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of warehouses with basic info.</returns>
        Task<IEnumerable<WarehouseResponseDto>> GetAllWarehousesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets warehouses with available capacity.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of available warehouses.</returns>
        Task<IEnumerable<WarehouseResponseDto>> GetAvailableWarehousesAsync(CancellationToken cancellationToken = default);
    }
}
