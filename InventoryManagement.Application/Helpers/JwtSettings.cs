namespace InventoryManagement.Application.Helpers
{
    /// <summary>
    /// JWT settings for authentication configuration.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Gets or sets the secret key for JWT signing.
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the access token expiration in minutes.
        /// </summary>
        public int AccessTokenExpiryMinutes { get; set; }

        /// <summary>
        /// Gets or sets the refresh token expiration in days.
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; }
    }
}
