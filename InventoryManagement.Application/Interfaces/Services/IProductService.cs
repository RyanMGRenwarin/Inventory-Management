using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for product management.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Gets a paginated list of products based on filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated product list.</returns>
        Task<ProductListResponseDto> GetProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a product by its ID.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Product details if found, otherwise null.</returns>
        Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="createDto">Product creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created product.</returns>
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="updateDto">Product update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated product.</returns>
        Task<ProductResponseDto> UpdateProductAsync(ProductUpdateDto updateDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a product by its ID (soft delete).
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, otherwise false.</returns>
        Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all products for dropdown/selection purposes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of products with basic info.</returns>
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a SKU is available.
        /// </summary>
        /// <param name="sku">SKU to check.</param>
        /// <param name="excludeProductId">Optional product ID to exclude.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if SKU is available, otherwise false.</returns>
        Task<bool> IsSkuAvailableAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets products with low stock.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of low stock products.</returns>
        Task<IEnumerable<ProductResponseDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    }
}
