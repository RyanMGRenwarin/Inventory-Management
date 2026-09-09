using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Category entity operations.
    /// </summary>
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        /// <summary>
        /// Gets a category by its name.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The category if found, otherwise null.</returns>
        Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets categories with product counts.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of categories with product counts.</returns>
        Task<IEnumerable<(Category Category, int ProductCount)>> GetCategoriesWithProductCountsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a category name already exists (excluding a specific category ID).
        /// </summary>
        /// <param name="name">The category name to check.</param>
        /// <param name="excludeCategoryId">Optional category ID to exclude from check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the category name exists, otherwise false.</returns>
        Task<bool> NameExistsAsync(string name, int? excludeCategoryId = null, CancellationToken cancellationToken = default);
    }
}
