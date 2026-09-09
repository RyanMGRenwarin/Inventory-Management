using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Warehouse entity.
    /// </summary>
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        /// <summary>
        /// Initializes a new instance of the WarehouseRepository class.
        /// </summary>
        /// <param name="context">Database context.</param>
        public WarehouseRepository(AppDbContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Warehouse>> GetAvailableWarehousesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(w => w.CurrentOccupancy < w.Capacity && !w.IsDeleted)
                .OrderBy(w => w.CurrentOccupancy)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateOccupancyAsync(int warehouseId, int changeInOccupancy, CancellationToken cancellationToken = default)
        {
            var warehouse = await GetByIdAsync(warehouseId, cancellationToken);
            if (warehouse != null)
            {
                warehouse.CurrentOccupancy += changeInOccupancy;
                warehouse.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(warehouse, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Warehouse>> GetHighOccupancyWarehousesAsync(
            double percentageThreshold,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(w => !w.IsDeleted && w.Capacity > 0)
                .ToListAsync(cancellationToken)
                .ContinueWith(task =>
                {
                    var warehouses = task.Result;
                    return warehouses
                        .Where(w => (double)w.CurrentOccupancy / w.Capacity * 100 >= percentageThreshold)
                        .OrderByDescending(w => (double)w.CurrentOccupancy / w.Capacity);
                }, cancellationToken);
        }
    }
}
