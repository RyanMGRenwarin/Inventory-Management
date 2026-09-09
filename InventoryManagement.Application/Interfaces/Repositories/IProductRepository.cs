using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Product entity operations.
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>
        /// Gets a product by its SKU.
        /// </summary>
        /// <param name="sku">The SKU to search for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The product if found, otherwise null.</returns>
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets products by category ID.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of products in the category.</returns>
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets products with low stock (below minimum threshold).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of products with low stock.</returns>
        Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the stock quantity of a product.
        /// </summary>
        /// <param name="productId">The product ID.</param>
        /// <param name="newStockQuantity">The new stock quantity.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateStockAsync(int productId, int newStockQuantity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a SKU already exists (excluding a specific product ID).
        /// </summary>
        /// <param name="sku">The SKU to check.</param>
        /// <param name="excludeProductId">Optional product ID to exclude from check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the SKU exists, otherwise false.</returns>
        Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default);
    }
}
