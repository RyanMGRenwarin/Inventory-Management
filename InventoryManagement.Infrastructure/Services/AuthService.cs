using InventoryManagement.Application.DTOs.Auth;
using InventoryManagement.Application.Helpers;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Authentication service implementation for JWT-based authentication.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthService class.
        /// </summary>
        /// <param name="unitOfWork">Unit of work instance.</param>
        /// <param name="jwtSettings">JWT settings.</param>
        /// <param name="logger">Logger instance.</param>
        public AuthService(
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, string ipAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Login attempt started: Username={Username}, IpAddress={IpAddress}",
                    request.Username, ipAddress);

                var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning(
                        "Login attempt failed: User not found or inactive. Username={Username}, IpAddress={IpAddress}",
                        request.Username, ipAddress);
                    return null;
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning(
                        "Login attempt failed: Invalid password. Username={Username}, UserId={UserId}, IpAddress={IpAddress}",
                        request.Username, user.Id, ipAddress);
                    return null;
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                    IsRevoked = false,
                    CreatedByIp = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddRefreshTokenAsync(refreshTokenEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Login successful: Username={Username}, UserId={UserId}, Role={Role}, IpAddress={IpAddress}, RefreshTokenExpiry={RefreshTokenExpiry}",
                    user.Username, user.Id, user.Role, ipAddress, refreshTokenEntity.ExpiryDate);

                return new LoginResponseDto
                {
                    AccessToken = accessToken,
                    TokenType = "Bearer",
                    ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                    RefreshToken = refreshToken,
                    FullName = user.FullName,
                    Role = user.Role.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during login: Username={Username}, IpAddress={IpAddress}",
                    request.Username, ipAddress);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<RefreshTokenResponseDto?> RefreshTokenAsync(string refreshToken, string ipAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Refresh token attempt started: IpAddress={IpAddress}",
                    ipAddress);

                var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning(
                        "Refresh token attempt failed: User not found. IpAddress={IpAddress}",
                        ipAddress);
                    return null;
                }

                var tokenEntity = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && !rt.IsRevoked);

                if (tokenEntity == null)
                {
                    _logger.LogWarning(
                        "Refresh token attempt failed: Token not found or revoked. UserId={UserId}, IpAddress={IpAddress}",
                        user.Id, ipAddress);
                    return null;
                }

                if (tokenEntity.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning(
                        "Refresh token attempt failed: Token expired. UserId={UserId}, ExpiredAt={ExpiredAt}, IpAddress={IpAddress}",
                        user.Id, tokenEntity.ExpiryDate, ipAddress);
                    return null;
                }

                // Generate new tokens
                var newAccessToken = GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Rotate refresh token (one-time use)
                tokenEntity.IsRevoked = true;
                tokenEntity.RevokedAt = DateTime.UtcNow;
                tokenEntity.RevokedByIp = ipAddress;
                tokenEntity.ReplacedByToken = newRefreshToken;

                // Save new refresh token
                var newTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                    IsRevoked = false,
                    CreatedByIp = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };

                user.RefreshTokens.Add(newTokenEntity);
                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Refresh token rotated successfully: UserId={UserId}, OldTokenRevokedAt={RevokedAt}, NewTokenExpiry={NewTokenExpiry}, IpAddress={IpAddress}",
                    user.Id, tokenEntity.RevokedAt, newTokenEntity.ExpiryDate, ipAddress);

                return new RefreshTokenResponseDto
                {
                    AccessToken = newAccessToken,
                    TokenType = "Bearer",
                    ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during token refresh: IpAddress={IpAddress}",
                    ipAddress);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> LogoutAsync(string refreshToken, string ipAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Logout attempt started: IpAddress={IpAddress}",
                    ipAddress);

                var user = await _unitOfWork.Users.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning(
                        "Logout attempt failed: User not found. IpAddress={IpAddress}",
                        ipAddress);
                    return false;
                }

                var tokenEntity = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && !rt.IsRevoked);

                if (tokenEntity != null)
                {
                    tokenEntity.IsRevoked = true;
                    tokenEntity.RevokedAt = DateTime.UtcNow;
                    tokenEntity.RevokedByIp = ipAddress;
                    await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Logout successful: Username={Username}, UserId={UserId}, RevokedAt={RevokedAt}, IpAddress={IpAddress}",
                        user.Username, user.Id, tokenEntity.RevokedAt, ipAddress);
                    return true;
                }

                _logger.LogWarning(
                    "Logout attempt failed: Token not found or already revoked. UserId={UserId}, IpAddress={IpAddress}",
                    user.Id, ipAddress);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during logout: IpAddress={IpAddress}",
                    ipAddress);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeAllTokensAsync(int userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Revoke all tokens attempt started: UserId={UserId}",
                    userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning(
                        "Revoke all tokens failed: User not found. UserId={UserId}",
                        userId);
                    return false;
                }

                var activeTokens = user.RefreshTokens.Where(rt => !rt.IsRevoked).ToList();

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "All tokens revoked successfully: Username={Username}, UserId={UserId}, RevokedCount={RevokedCount}, RevokedAt={RevokedAt}",
                    user.Username, user.Id, activeTokens.Count, DateTime.UtcNow);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error revoking all tokens: UserId={UserId}",
                    userId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RegisterAsync(RegisterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "User registration attempt started: Username={Username}, Email={Email}",
                    request.Username, request.Email);

                // Check if username already exists
                if (await _unitOfWork.Users.UsernameExistsAsync(request.Username, cancellationToken))
                {
                    _logger.LogWarning(
                        "Registration failed: Username already exists. Username={Username}",
                        request.Username);
                    return false;
                }

                // Check if email already exists
                if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
                {
                    _logger.LogWarning(
                        "Registration failed: Email already exists. Email={Email}",
                        request.Email);
                    return false;
                }

                // Create new user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = UserRole.User, // Default role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "User registered successfully: Username={Username}, UserId={UserId}, Email={Email}, Role={Role}",
                    user.Username, user.Id, user.Email, user.Role);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error during registration: Username={Username}, Email={Email}",
                    request.Username, request.Email);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Change password attempt started: UserId={UserId}",
                    userId);

                var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning(
                        "Change password failed: User not found. UserId={UserId}",
                        userId);
                    return false;
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    _logger.LogWarning(
                        "Change password failed: Invalid current password. UserId={UserId}, Username={Username}",
                        userId, user.Username);
                    return false;
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Password changed successfully: UserId={UserId}, Username={Username}, ChangedAt={ChangedAt}",
                    userId, user.Username, user.UpdatedAt);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error changing password: UserId={UserId}",
                    userId);
                throw;
            }
        }

        /// <summary>
        /// Generates a JWT access token for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The JWT token string.</returns>
        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token.
        /// </summary>
        /// <returns>The refresh token string.</returns>
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
