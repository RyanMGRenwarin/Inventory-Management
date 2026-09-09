using InventoryManagement.Application.DTOs.Category;

namespace InventoryManagement.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for category management.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Gets a paginated list of categories based on filter criteria.
        /// </summary>
        /// <param name="filter">Filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated category list.</returns>
        Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync(CategoryFilterDto filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a category by its ID.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Category details if found, otherwise null.</returns>
        Task<CategoryResponseDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="createDto">Category creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created category.</returns>
        Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto createDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="updateDto">Category update data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated category.</returns>
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryCreateDto updateDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a category by its ID (soft delete).
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deleted successfully, otherwise false.</returns>
        Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all categories for dropdown/selection purposes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of categories with basic info.</returns>
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a category name is available.
        /// </summary>
        /// <param name="name">Category name to check.</param>
        /// <param name="excludeCategoryId">Optional category ID to exclude.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if name is available, otherwise false.</returns>
        Task<bool> IsCategoryNameAvailableAsync(string name, int? excludeCategoryId = null, CancellationToken cancellationToken = default);
    }
}
