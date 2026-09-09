namespace InventoryManagement.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for refresh token response.
    /// </summary>
    public class RefreshTokenResponseDto
    {
        /// <summary>
        /// Gets or sets the new access token.
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
        /// Gets or sets the new refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
