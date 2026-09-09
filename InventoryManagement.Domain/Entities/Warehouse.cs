using InventoryManagement.Domain.Entities.Base;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents a warehouse where products are stored.
    /// </summary>
    public class Warehouse : BaseEntity
    {
        /// <summary>
        /// Gets or sets the warehouse name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warehouse location.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum capacity of the warehouse.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the current occupancy count.
        /// </summary>
        public int CurrentOccupancy { get; set; }

        /// <summary>
        /// Gets or sets the collection of inventory transactions for this warehouse.
        /// </summary>
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
