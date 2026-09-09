using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Gets a user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user if found, otherwise null.</returns>
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user if found, otherwise null.</returns>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user with their refresh tokens.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user with refresh tokens if found, otherwise null.</returns>
        Task<User?> GetUserWithRefreshTokensAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The user if found, otherwise null.</returns>
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a username already exists.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the username exists, otherwise false.</returns>
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an email already exists.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the email exists, otherwise false.</returns>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new refresh token for a user.
        /// </summary>
        /// <param name="refreshToken">The refresh token entity to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    }
}
