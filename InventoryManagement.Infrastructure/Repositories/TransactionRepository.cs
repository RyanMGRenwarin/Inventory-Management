using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for InventoryTransaction entity.
    /// </summary>
    public class TransactionRepository : GenericRepository<InventoryTransaction>, ITransactionRepository
    {
        /// <summary>
        /// Initializes a new instance of the TransactionRepository class.
        /// </summary>
        /// <param name="context">Database context.</param>
        public TransactionRepository(AppDbContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InventoryTransaction>> GetByProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.CreatedByUser)
                .Where(t => t.ProductId == productId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.CreatedByUser)
                .Where(t => t.WarehouseId == warehouseId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.CreatedByUser)
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(TransactionType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Product)
                .Include(t => t.Warehouse)
                .Include(t => t.CreatedByUser)
                .Where(t => t.Type == type && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> GetTotalQuantityMovedAsync(
            int productId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var transactions = await _dbSet
                .Where(t => t.ProductId == productId &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate &&
                           !t.IsDeleted)
                .ToListAsync(cancellationToken);

            return transactions.Sum(t => t.Quantity);
        }
    }
}
