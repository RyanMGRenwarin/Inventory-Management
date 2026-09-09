namespace InventoryManagement.Application.DTOs.Product
{
    /// <summary>
    /// DTO for filtering products.
    /// </summary>
    public class ProductFilterDto
    {
        /// <summary>
        /// Gets or sets the search term for product name or SKU.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the category ID filter.
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the minimum price filter.
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Gets or sets the maximum price filter.
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show only low stock items.
        /// </summary>
        public bool? LowStockOnly { get; set; }

        /// <summary>
        /// Gets or sets the page number (1-based).
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sort column.
        /// </summary>
        public string? SortColumn { get; set; }

        /// <summary>
        /// Gets or sets the sort direction (asc/desc).
        /// </summary>
        public string? SortDirection { get; set; }
    }
}
