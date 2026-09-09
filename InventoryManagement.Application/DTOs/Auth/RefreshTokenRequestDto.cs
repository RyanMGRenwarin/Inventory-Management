using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for refresh token request.
    /// </summary>

    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
