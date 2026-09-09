using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Warehouse
{
    /// <summary>
    /// DTO for creating a new warehouse.
    /// </summary>
    public class WarehouseCreateDto
    {
        /// <summary>
        /// Gets or sets the warehouse name.
        /// </summary>
        [Required(ErrorMessage = "Warehouse name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Warehouse name must be between 2 and 100 characters")]
        [DisplayName("Warehouse Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warehouse location.
        /// </summary>
        [Required(ErrorMessage = "Location is required")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        [DisplayName("Location")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warehouse capacity.
        /// </summary>
        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        [DisplayName("Capacity")]
        public int Capacity { get; set; }
    }
}
