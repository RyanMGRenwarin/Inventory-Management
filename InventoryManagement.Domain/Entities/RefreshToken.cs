using InventoryManagement.Domain.Entities.Base;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token for JWT authentication.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>
        /// Gets or sets the foreign key for the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with this refresh token.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the refresh token value.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token expiration date.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token is revoked.
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the token was revoked.
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the token was created.
        /// </summary>
        public string? CreatedByIp { get; set; }

        /// <summary>
        /// Gets or sets the IP address from which the token was revoked.
        /// </summary>
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Gets or sets the token that replaced this token (for rotation).
        /// </summary>
        public string? ReplacedByToken { get; set; }
    }
}
