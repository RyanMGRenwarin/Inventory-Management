using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Web.ViewModels.Warehouse
{
    /// <summary>
    /// View model for warehouse create page.
    /// </summary>
    public class WarehouseCreateViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the warehouse creation data.
        /// </summary>
        public WarehouseCreateDto Warehouse { get; set; } = new();
    }
}
