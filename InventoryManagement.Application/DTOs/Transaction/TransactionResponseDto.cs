using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Application.DTOs.Transaction
{
    /// <summary>
    /// DTO for transaction response data.
    /// </summary>
    public class TransactionResponseDto
    {
        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warehouse ID.
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse name.
        /// </summary>
        public string WarehouseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction type.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the type display name.
        /// </summary>
        public string TypeDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user who created the transaction.
        /// </summary>
        public string CreatedByUserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
