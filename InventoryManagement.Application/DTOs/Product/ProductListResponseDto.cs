namespace InventoryManagement.Application.DTOs.Product
{
    /// <summary>
    /// DTO for paginated product list response.
    /// </summary>
    public class ProductListResponseDto
    {
        /// <summary>
        /// Gets or sets the list of products.
        /// </summary>
        public IEnumerable<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();

        /// <summary>
        /// Gets or sets the total number of products.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
