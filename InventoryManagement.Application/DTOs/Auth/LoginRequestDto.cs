using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for user login request.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to remember the user.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
