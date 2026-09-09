using InventoryManagement.Application.Interfaces.Repositories;

namespace InventoryManagement.Application.Interfaces
{

    /// <summary>
    /// Unit of Work pattern interface for managing transactions and repositories.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the product repository.
        /// </summary>
        IProductRepository Products { get; }

        /// <summary>
        /// Gets the warehouse repository.
        /// </summary>
        IWarehouseRepository Warehouses { get; }

        /// <summary>
        /// Gets the transaction repository.
        /// </summary>
        ITransactionRepository Transactions { get; }

        /// <summary>
        /// Gets the category repository.
        /// </summary>
        ICategoryRepository Categories { get; }

        /// <summary>
        /// Gets the user repository.
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// Saves all changes made in the context.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of affected rows.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A transaction object.</returns>
        Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a database transaction.
    /// </summary>
    public interface IDatabaseTransaction : IDisposable
    {
        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
