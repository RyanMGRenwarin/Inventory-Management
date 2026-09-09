namespace InventoryManagement.Web.ViewModels
{
    /// <summary>
    /// Base view model with common properties.
    /// </summary>
    public abstract class BaseViewModel
    {
        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public string? UserRole { get; set; }

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string? UserFullName { get; set; }
    }
}
