using InventoryManagement.Application.DTOs.Transaction;

namespace InventoryManagement.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for transaction management.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Gets a paginated list of transactions based on filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated transaction list.</returns>
        Task<TransactionListResponseDto> GetTransactionsAsync(TransactionFilterDto filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a transaction by its ID.
        /// </summary>
        /// <param name="id">Transaction ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transaction details if found, otherwise null.</returns>
        Task<TransactionResponseDto?> GetTransactionByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new inventory transaction.
        /// </summary>
        /// <param name="createDto">Transaction creation data.</param>
        /// <param name="userId">ID of the user creating the transaction.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created transaction.</returns>
        Task<TransactionResponseDto> CreateTransactionAsync(TransactionCreateDto createDto, int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a transaction by its ID (soft delete).
        /// </summary>
        /// <param name="id">Transaction ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, otherwise false.</returns>
        Task<bool> DeleteTransactionAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets transaction summary for dashboard.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Transaction summary data.</returns>
        Task<TransactionSummaryDto> GetTransactionSummaryAsync(CancellationToken cancellationToken = default);
    }
}
