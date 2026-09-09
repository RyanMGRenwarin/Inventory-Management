namespace InventoryManagement.Application.DTOs.Warehouse
{
    /// <summary>
    /// DTO for filtering warehouses.
    /// </summary>
    public class WarehouseFilterDto
    {
        /// <summary>
        /// Gets or sets the search term for warehouse name or location.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show only available warehouses (with capacity).
        /// </summary>
        public bool? AvailableOnly { get; set; }

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
