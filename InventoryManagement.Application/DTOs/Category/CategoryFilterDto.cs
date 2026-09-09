namespace InventoryManagement.Application.DTOs.Category
{
    /// <summary>
    /// DTO for filtering categories.
    /// </summary>
    public class CategoryFilterDto
    {
        /// <summary>
        /// Gets or sets the search term for category name.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the page number (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
