namespace InventoryManagement.Application.DTOs.Warehouse
{
    /// <summary>
    /// DTO for warehouse response data.
    /// </summary>
    public class WarehouseResponseDto
    {
        /// <summary>
        /// Gets or sets the warehouse ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the warehouse name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warehouse location.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warehouse capacity.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the current occupancy.
        /// </summary>
        public int CurrentOccupancy { get; set; }

        /// <summary>
        /// Gets or sets the occupancy percentage.
        /// </summary>
        public double OccupancyPercentage { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
