using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for InventoryTransaction entity operations.
    /// </summary>
    public interface ITransactionRepository : IGenericRepository<InventoryTransaction>
    {
        /// <summary>
        /// Gets transactions for a specific product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of transactions for the product.</returns>
        Task<IEnumerable<InventoryTransaction>> GetByProductAsync(int productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets transactions for a specific warehouse.
        /// </summary>
        /// <param name="warehouseId">The warehouse ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of transactions for the warehouse.</returns>
        Task<IEnumerable<InventoryTransaction>> GetByWarehouseAsync(int warehouseId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets transactions by date range.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of transactions in the date range.</returns>
        Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets transactions by type.
        /// </summary>
        /// <param name="type">The transaction type.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of transactions of the specified type.</returns>
        Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(TransactionType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the total quantity moved for a product in a date range.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Total quantity moved.</returns>
        Task<int> GetTotalQuantityMovedAsync(
            int productId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);
    }
}
