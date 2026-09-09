using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Category entity.
    /// </summary>
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        /// <summary>
        /// Initializes a new instance of the CategoryRepository class.
        /// </summary>
        /// <param name="context">Database context.</param>
        public CategoryRepository(AppDbContext context)
            : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(Category Category, int ProductCount)>> GetCategoriesWithProductCountsAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _dbSet
                .Include(c => c.Products.Where(p => !p.IsDeleted))
                .Where(c => !c.IsDeleted)
                .ToListAsync(cancellationToken);

            return categories.Select(c => (c, c.Products.Count(p => !p.IsDeleted)));
        }

        /// <inheritdoc/>
        public async Task<bool> NameExistsAsync(string name, int? excludeCategoryId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(c => c.Name == name && !c.IsDeleted);

            if (excludeCategoryId.HasValue)
            {
                query = query.Where(c => c.Id != excludeCategoryId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
    }
}
