using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Web.ViewModels.Warehouse
{
    /// <summary>
    /// View model for warehouse edit page.
    /// </summary>
    public class WarehouseEditViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the warehouse ID.
        /// </summary>
        public int WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the warehouse update data.
        /// </summary>
        public WarehouseCreateDto Warehouse { get; set; } = new();
    }
}
