using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for forgot password request.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        /// <summary>
        /// Gets or sets the email address for password reset.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [DisplayName("Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
