using InventoryManagement.Domain.Entities.Base;

namespace InventoryManagement.Domain.Entities
{
    /// <summary>
    /// Represents a product category.
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the collection of products in this category.
        /// </summary>
        public virtual ICollection<Product> Products { get; set; } = [];
    }
}
