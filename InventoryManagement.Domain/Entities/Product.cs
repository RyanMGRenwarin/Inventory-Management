using InventoryManagement.Domain.Entities.Base;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents a product in the inventory management system.
    /// </summary>
    public class Product : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SKU (Stock Keeping Unit) identifier.
        /// </summary>
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

         /// <summary>
         /// Gets or sets the unit price of the product.
         /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the current stock quantity.
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the minimum stock threshold for alerts.
        /// </summary>
        public int MinimumStockThreshold { get; set; }

        /// <summary>
        /// Gets or sets the foreign key for the category.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category associated with the product.
        /// </summary>
        public virtual Category Category { get; set; } = null!;

        /// <summary>
        /// Gets or sets the collection of inventory transactions for this product.
        /// </summary>
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
