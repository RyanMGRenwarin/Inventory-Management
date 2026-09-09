using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.DTOs.Transaction
{
    /// <summary>
    /// DTO for filtering transactions.
    /// </summary>
    public class TransactionFilterDto
    {
        /// <summary>
        /// Gets or sets the product ID filter.
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse ID filter.
        /// </summary>
        public int? WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the transaction type filter.
        /// </summary>
        public TransactionType? Type { get; set; }

        /// <summary>
        /// Gets or sets the start date filter.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date filter.
        /// </summary>
        public DateTime? EndDate { get; set; }

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
