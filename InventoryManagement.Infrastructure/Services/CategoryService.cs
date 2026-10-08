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
                _logger.LogDebug("Retrieving categories: SearchTerm={SearchTerm}", filter.SearchTerm);

                var categoriesWithCounts = await _unitOfWork.Categories.GetCategoriesWithProductCountsAsync(
                    cancellationToken);

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
                    .OrderBy(c => c.Name)
                    .ToList();

                _logger.LogInformation("Categories retrieved successfully: Count={Count}, SearchTerm={SearchTerm}",
                    result.Count, filter.SearchTerm);

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
                    _logger.LogWarning("Category not found: CategoryId={CategoryId}", id);
                    return null;
                }

                var productCount = await _unitOfWork.Products.CountAsync(p => p.CategoryId == id, cancellationToken);

                _logger.LogDebug("Category retrieved: CategoryId={CategoryId}, Name={CategoryName}, ProductCount={ProductCount}",
                    category.Id, category.Name, productCount);

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
                _logger.LogInformation("Creating category: Name={CategoryName}", createDto.Name);

                // Check if category name already exists
                if (await _unitOfWork.Categories.NameExistsAsync(createDto.Name, null, cancellationToken))
                {
                    _logger.LogWarning("Category creation failed: Duplicate name. Name={CategoryName}", createDto.Name);
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

                _logger.LogInformation("Category created successfully: CategoryId={CategoryId}, Name={CategoryName}",
                    category.Id, category.Name);

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
                _logger.LogInformation("Updating category: CategoryId={CategoryId}, Name={CategoryName}",
                    id, updateDto.Name);

                var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    _logger.LogWarning("Category update failed: Category not found. CategoryId={CategoryId}", id);
                    throw new InvalidOperationException($"Category with ID {id} does not exist.");
                }

                // Check if category name already exists (excluding current)
                if (await _unitOfWork.Categories.NameExistsAsync(updateDto.Name, id, cancellationToken))
                {
                    _logger.LogWarning("Category update failed: Duplicate name. Name={CategoryName}, ExcludeCategoryId={CategoryId}",
                        updateDto.Name, id);
                    throw new InvalidOperationException($"Category name '{updateDto.Name}' already exists.");
                }

                category.Name = updateDto.Name;
                category.Description = updateDto.Description;
                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var productCount = await _unitOfWork.Products.CountAsync(p => p.CategoryId == id, cancellationToken);

                _logger.LogInformation("Category updated successfully: CategoryId={CategoryId}, Name={CategoryName}, ProductCount={ProductCount}",
                    category.Id, category.Name, productCount);

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
                _logger.LogError(ex, "Error updating category: CategoryId={CategoryId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCategoryAsync(int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting category: CategoryId={CategoryId}", id);

                var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
                if (category == null)
                {
                    _logger.LogWarning("Category deletion failed: Category not found. CategoryId={CategoryId}", id);
                    return false;
                }

                // Check if category has products
                var hasProducts = await _unitOfWork.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
                if (hasProducts)
                {
                    _logger.LogWarning("Category deletion failed: Category has existing products. CategoryId={CategoryId}, Name={CategoryName}",
                        id, category.Name);
                    throw new InvalidOperationException("Cannot delete category with existing products.");
                }

                await _unitOfWork.Categories.DeleteByIdAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Category deleted successfully: CategoryId={CategoryId}, Name={CategoryName}",
                    id, category.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: CategoryId={CategoryId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var categoriesWithCounts = await _unitOfWork.Categories.GetCategoriesWithProductCountsAsync(cancellationToken);

                var result = categoriesWithCounts
                    .Select(t => new CategoryResponseDto
                    {
                        Id = t.Category.Id,
                        Name = t.Category.Name,
                        Description = t.Category.Description,
                        ProductCount = t.ProductCount,
                        CreatedAt = t.Category.CreatedAt
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                _logger.LogDebug("All categories retrieved: Count={Count}", result.Count);

                return result;
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
                var isAvailable = !await _unitOfWork.Categories.NameExistsAsync(name, excludeCategoryId, cancellationToken);

                _logger.LogDebug("Category name availability checked: Name={Name}, ExcludeCategoryId={ExcludeCategoryId}, IsAvailable={IsAvailable}",
                    name, excludeCategoryId, isAvailable);

                return isAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking category name availability: {Name}", name);
                throw;
            }
        }
    }
}
