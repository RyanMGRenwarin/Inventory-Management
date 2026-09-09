using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Application.Interfaces.Services;
using InventoryManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for product management.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        /// <summary>
        /// Initializes a new instance of the ProductService class.
        /// </summary>
        /// <param name="unitOfWork">Unit of work instance.</param>
        /// <param name="logger">Logger instance.</param>
        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ProductListResponseDto> GetProductsAsync(ProductFilterDto filter, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Build filter expression
                Expression<Func<Product, bool>>? predicate = null;

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    predicate = p => p.Name.ToLower().Contains(searchTerm) ||
                                     p.SKU.ToLower().Contains(searchTerm);
                }

                if (filter.CategoryId.HasValue)
                {
                    var categoryId = filter.CategoryId.Value;
                    predicate = predicate == null
                        ? p => p.CategoryId == categoryId
                        : CombinePredicates(predicate, p => p.CategoryId == categoryId);
                }

                if (filter.MinPrice.HasValue)
                {
                    var minPrice = filter.MinPrice.Value;
                    predicate = predicate == null
                        ? p => p.Price >= minPrice
                        : CombinePredicates(predicate, p => p.Price >= minPrice);
                }

                if (filter.MaxPrice.HasValue)
                {
                    var maxPrice = filter.MaxPrice.Value;
                    predicate = predicate == null
                        ? p => p.Price <= maxPrice
                        : CombinePredicates(predicate, p => p.Price <= maxPrice);
                }

                if (filter.LowStockOnly == true)
                {
                    predicate = predicate == null
                        ? p => p.StockQuantity <= p.MinimumStockThreshold
                        : CombinePredicates(predicate, p => p.StockQuantity <= p.MinimumStockThreshold);
                }

                // Get products with pagination
                var (products, totalCount) = await _unitOfWork.Products.GetPagedAsync(
                    filter.PageNumber,
                    filter.PageSize,
                    predicate,
                    cancellationToken: cancellationToken);

                var productDtos = products.Select(p => MapToResponseDto(p));

                return new ProductListResponseDto
                {
                    Products = productDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products with filter: {@Filter}", filter);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ProductResponseDto?> GetProductByIdAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
                return product != null ? MapToResponseDto(product) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by ID: {ProductId}", id);
                throw;
            }
        }

        /// <inheritdoc/>      
        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if SKU already exists
                if (await _unitOfWork.Products.SkuExistsAsync(createDto.SKU, null, cancellationToken))
                {
                    throw new InvalidOperationException($"SKU '{createDto.SKU}' already exists.");
                }

                // Check if category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(createDto.CategoryId, cancellationToken);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with ID {createDto.CategoryId} does not exist.");
                }

                var product = new Product
                {
                    Name = createDto.Name,
                    SKU = createDto.SKU,
                    Description = createDto.Description,
                    Price = createDto.Price,
                    StockQuantity = createDto.StockQuantity,
                    MinimumStockThreshold = createDto.MinimumStockThreshold,
                    CategoryId = createDto.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Products.AddAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product created successfully: {ProductName} (SKU: {SKU})", product.Name, product.SKU);
                return MapToResponseDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {@CreateDto}", createDto);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ProductResponseDto> UpdateProductAsync(ProductUpdateDto updateDto, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(updateDto.Id, cancellationToken);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {updateDto.Id} does not exist.");
                }

                // Check if SKU already exists (excluding current product)
                if (await _unitOfWork.Products.SkuExistsAsync(updateDto.SKU, updateDto.Id, cancellationToken))
                {
                    throw new InvalidOperationException($"SKU '{updateDto.SKU}' already exists.");
                }

                // Check if category exists
                var category = await _unitOfWork.Categories.GetByIdAsync(updateDto.CategoryId, cancellationToken);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with ID {updateDto.CategoryId} does not exist.");
                }

                product.Name = updateDto.Name;
                product.SKU = updateDto.SKU;
                product.Description = updateDto.Description;
                product.Price = updateDto.Price;
                product.StockQuantity = updateDto.StockQuantity;
                product.MinimumStockThreshold = updateDto.MinimumStockThreshold;
                product.CategoryId = updateDto.CategoryId;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product updated successfully: {ProductName} (ID: {ProductId})", product.Name, product.Id);
                return MapToResponseDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {@UpdateDto}", updateDto);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteProductAsync(int id, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning("Product not found for deletion: {ProductId}", id);
                    return false;
                }

                await _unitOfWork.Products.DeleteByIdAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product deleted successfully: {ProductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync(null, cancellationToken);
                return products.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsSkuAvailableAsync(string sku, int? excludeProductId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                return !await _unitOfWork.Products.SkuExistsAsync(sku, excludeProductId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SKU availability: {SKU}", sku);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ProductResponseDto>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);
                return products.Select(MapToResponseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                throw;
            }
        }

        /// <summary>
        /// Combines two predicate expressions with AND logic.
        /// </summary>
        private static Expression<Func<T, bool>> CombinePredicates<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter));
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        /// <summary>
        /// Maps a Product entity to ProductResponseDto.
        /// </summary>
        private static ProductResponseDto MapToResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MinimumStockThreshold = product.MinimumStockThreshold,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
