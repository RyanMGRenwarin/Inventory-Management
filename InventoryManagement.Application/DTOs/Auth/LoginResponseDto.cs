namespace InventoryManagement.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for login response containing JWT tokens.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token type (Bearer).
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Gets or sets the access token expiration in seconds.
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
