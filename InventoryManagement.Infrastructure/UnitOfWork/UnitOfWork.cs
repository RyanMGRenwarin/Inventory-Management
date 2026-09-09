using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventoryManagement.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unit of Work implementation for managing transactions and repositories.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        private IProductRepository? _productRepository;
        private IWarehouseRepository? _warehouseRepository;
        private ITransactionRepository? _transactionRepository;
        private ICategoryRepository? _categoryRepository;
        private IUserRepository? _userRepository;

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">Database context.</param>
        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public IProductRepository Products =>
            _productRepository ??= new ProductRepository(_context);

        /// <inheritdoc/>
        public IWarehouseRepository Warehouses =>
            _warehouseRepository ??= new WarehouseRepository(_context);

        /// <inheritdoc/>
        public ITransactionRepository Transactions =>
            _transactionRepository ??= new TransactionRepository(_context);

        /// <inheritdoc/>
        public ICategoryRepository Categories =>
            _categoryRepository ??= new CategoryRepository(_context);

        /// <inheritdoc/>
        public IUserRepository Users =>
            _userRepository ??= new UserRepository(_context);

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IDatabaseTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new DatabaseTransaction(_transaction);
        }

        /// <summary>
        /// Disposes the Unit of Work.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the Unit of Work.
        /// </summary>
        /// <param name="disposing">Indicates whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Database transaction implementation.
    /// </summary>
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly IDbContextTransaction _transaction;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the DatabaseTransaction class.
        /// </summary>
        /// <param name="transaction">The database transaction.</param>
        public DatabaseTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <inheritdoc/>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }

        /// <summary>
        /// Disposes the transaction.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the transaction.
        /// </summary>
        /// <param name="disposing">Indicates whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction.Dispose();
                }
                _disposed = true;
            }
        }
    }
}