using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Product entity.
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        /// <summary>
        /// Initializes a new instance of the ProductRepository class.
        /// </summary>
        /// <param name="context">Database context.</param>
        public ProductRepository(AppDbContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.SKU == sku && !p.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= p.MinimumStockThreshold && !p.IsDeleted)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateStockAsync(int productId, int newStockQuantity, CancellationToken cancellationToken = default)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                product.StockQuantity = newStockQuantity;
                product.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(product, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.SKU == sku && !p.IsDeleted);

            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
    }
}
