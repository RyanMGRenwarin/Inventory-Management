using InventoryManagement.Application.DTOs.Auth;

namespace InventoryManagement.Application.Interfaces
{
    /// <summary>
    /// Authentication service interface for JWT-based authentication.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user and returns JWT tokens.
        /// </summary>
        /// <param name="request">Login request.</param>
        /// <param name="ipAddress">Client IP address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Login response with tokens if successful, otherwise null.</returns>
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string ipAddress, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="ipAddress">Client IP address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Refresh response with new tokens if successful, otherwise null.</returns>
        Task<RefreshTokenResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out a user by revoking their refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to revoke.</param>
        /// <param name="ipAddress">Client IP address.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if logout successful, otherwise false.</returns>
        Task<bool> LogoutAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all refresh tokens for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if tokens were revoked, otherwise false.</returns>
        Task<bool> RevokeAllTokensAsync(int userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">Registration request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if registration successful, otherwise false.</returns>
        Task<bool> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if password was changed, otherwise false.</returns>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    }
}
