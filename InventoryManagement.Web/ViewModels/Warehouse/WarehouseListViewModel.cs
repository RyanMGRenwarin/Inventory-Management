using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Web.ViewModels.Warehouse
{
    /// <summary>
    /// View model for warehouse list page.
    /// </summary>
    public class WarehouseListViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the list of warehouses.
        /// </summary>
        public IEnumerable<WarehouseResponseDto> Warehouses { get; set; } = new List<WarehouseResponseDto>();

        /// <summary>
        /// Gets or sets the current filter criteria.
        /// </summary>
        public WarehouseFilterDto Filter { get; set; } = new();
    }
}
