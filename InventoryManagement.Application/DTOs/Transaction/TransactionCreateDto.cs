using InventoryManagement.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Transaction
{
    /// <summary>
    /// DTO for creating an inventory transaction.
    /// </summary>
    public class TransactionCreateDto
    {
        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Product is required")]
        [DisplayName("Product")]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse ID.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Warehouse is required")]
        [DisplayName("Warehouse")]
        public int WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the transaction type.
        /// </summary>
        [Required(ErrorMessage = "Transaction type is required")]
        [DisplayName("Transaction Type")]
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [DisplayName("Quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        [Required(ErrorMessage = "Unit price is required")]
        [DataType(DataType.Currency)]
        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the transaction notes.
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [DisplayName("Notes")]
        public string Notes { get; set; } = string.Empty;
    }
}
