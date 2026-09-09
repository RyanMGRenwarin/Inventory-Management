using System.Linq.Expressions;

namespace InventoryManagement.Application.Interfaces.Repositories
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities with optional filtering.
        /// </summary>
        /// <param name="predicate">Optional filter predicate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of entities.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="predicate">Optional filter predicate.</param>
        /// <param name="orderBy">Optional ordering expression.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated result containing entities and total count.</returns>
        Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        Task<TEntity?> GetSingleAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if any entity matches, otherwise false.</returns>
        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The added entity.</returns>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple entities.
        /// </summary>
        /// <param name="entities">The entities to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated entity.</returns>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity by its ID (soft delete).
        /// </summary>
        /// <param name="id">The entity ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities matching the predicate.
        /// </summary>
        /// <param name="predicate">Optional filter predicate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The count of entities.</returns>
        Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken cancellationToken = default);
    }
}
