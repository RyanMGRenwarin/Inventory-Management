using InventoryManagement.Domain.Entities.Base;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents an inventory transaction (stock in/out).
    /// </summary>
    public class InventoryTransaction : BaseEntity
    {
        /// <summary>
        /// Gets or sets the foreign key for the product.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product associated with the transaction.
        /// </summary>
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// Gets or sets the foreign key for the warehouse.
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse associated with the transaction.
        /// </summary>
        public virtual Warehouse Warehouse { get; set; } = null!;

        /// <summary>
        /// Gets or sets the transaction type (Inbound or Outbound).
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets or sets the quantity of products in the transaction.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price at the time of transaction.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total amount (Quantity * UnitPrice).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets any additional notes for the transaction.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the foreign key for the user who created the transaction.
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user who created the transaction.
        /// </summary>
        public virtual User CreatedByUser { get; set; } = null!;

    }
}
