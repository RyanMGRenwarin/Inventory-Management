using InventoryManagement.Application.Interfaces.Repositories;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for User entity.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the UserRepository class.
    /// </remarks>
    /// <param name="context">Database context.</param>
    public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
    {
        /// <inheritdoc/>
        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserWithRefreshTokensAsync(string username, CancellationToken cancellationToken = default)
        {
            var user = await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);

            if (user != null)
            {
                user.RefreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && !rt.IsRevoked && !rt.IsDeleted)
                    .ToListAsync(cancellationToken);
            }

            return user;
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken && !rt.IsRevoked && !rt.IsDeleted) && !u.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }
    }
}
