namespace InventoryManagement.Application.DTOs.Transaction
{
    /// <summary>
    /// DTO for paginated transaction list response.
    /// </summary>
    public class TransactionListResponseDto
    {
        /// <summary>
        /// Gets or sets the list of transactions.
        /// </summary>
        public IEnumerable<TransactionResponseDto> Transactions { get; set; } = new List<TransactionResponseDto>();

        /// <summary>
        /// Gets or sets the total number of transactions.
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
