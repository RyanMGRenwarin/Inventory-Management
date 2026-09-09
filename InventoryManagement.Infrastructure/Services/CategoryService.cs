using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for category management.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        /// <summary>
        /// Initializes a new instance of the CategoryService class.
        /// </summary>
        /// <param name="unitOfWork">Unit of work instance.</param>
        /// <param name="logger">Logger instance.</param>
        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync(CategoryFilterDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var categoriesWithCounts = await _unitOfWork.Categories.GetCategoriesWithProductCountsAsync(
                    cancellationToken = default);

                var searchTerm = string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.ToLower();
                var result = categoriesWithCounts
                    .Where(t => (searchTerm is null) ||
                                t.Category.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                    .Select(t => new CategoryResponseDto
                    {
                        Id = t.Category.Id,
                        Name = t.Category.Name,
                        Description = t.Category.Description,
                        ProductCount = t.ProductCount,
                        CreatedAt = t.Category.CreatedAt
                    })
                    .OrderBy(c => c.Name);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories with filter: {@Filter}", filter);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    return null;
                }

                var productCount = await _unitOfWork.Products.CountAsync(p => p.CategoryId == id, cancellationToken);

                return new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = productCount,
                    CreatedAt = category.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto createDto, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if category name already exists
                if (await _unitOfWork.Categories.NameExistsAsync(createDto.Name, null, cancellationToken))
                {
                    throw new InvalidOperationException($"Category name '{createDto.Name}' already exists.");
                }

                var category = new Category
                {
                    Name = createDto.Name,
                    Description = createDto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Categories.AddAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category created successfully: {CategoryName}", category.Name);

                return new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = 0,
                    CreatedAt = category.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {@CreateDto}", createDto);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryCreateDto updateDto, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with ID {id} does not exist.");
                }

                // Check if category name already exists (excluding current)
                if (await _unitOfWork.Categories.NameExistsAsync(updateDto.Name, id, cancellationToken))
                {
                    throw new InvalidOperationException($"Category name '{updateDto.Name}' already exists.");
                }

                category.Name = updateDto.Name;
                category.Description = updateDto.Description;
                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var productCount = await _unitOfWork.Products.CountAsync(p => p.CategoryId == id, cancellationToken);

                _logger.LogInformation("Category updated successfully: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

                return new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = productCount,
                    CreatedAt = category.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCategoryAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    _logger.LogWarning("Category not found for deletion: {CategoryId}", id);
                    return false;
                }

                // Check if category has products
                var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
                if (hasProducts)
                {
                    throw new InvalidOperationException("Cannot delete category with existing products.");
                }

                await _unitOfWork.Categories.DeleteByIdAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category deleted successfully: {CategoryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var categoriesWithCounts = await _unitOfWork.Categories.GetCategoriesWithProductCountsAsync(cancellationToken);
                return categoriesWithCounts
                    .Select(t => new CategoryResponseDto
                    {
                        Id = t.Category.Id,
                        Name = t.Category.Name,
                        Description = t.Category.Description,
                        ProductCount = t.ProductCount,
                        CreatedAt = t.Category.CreatedAt
                    })
                    .OrderBy(c => c.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsCategoryNameAvailableAsync(string name, 
            int? excludeCategoryId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _unitOfWork.Categories.NameExistsAsync(name, excludeCategoryId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking category name availability: {Name}", name);
                throw;
            }
        }
    }
}
