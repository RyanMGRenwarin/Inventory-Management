using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Warehouse entity operations.
    /// </summary>
    public interface IWarehouseRepository : IGenericRepository<Warehouse>
    {
        /// <summary>
        /// Gets warehouses with available capacity.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of warehouses with available capacity.</returns>
        Task<IEnumerable<Warehouse>> GetAvailableWarehousesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the occupancy of a warehouse.
        /// </summary>
        /// <param name="warehouseId">The warehouse ID.</param>
        /// <param name="changeInOccupancy">The change in occupancy (positive or negative).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateOccupancyAsync(int warehouseId, int changeInOccupancy, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets warehouses with occupancy above a certain percentage.
        /// </summary>
        /// <param name="percentageThreshold">The occupancy percentage threshold.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of warehouses with high occupancy.</returns>
        Task<IEnumerable<Warehouse>> GetHighOccupancyWarehousesAsync(
            double percentageThreshold,
            CancellationToken cancellationToken = default);
    }
}
