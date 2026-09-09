using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Web.ViewModels.Transaction
{
    /// <summary>
    /// View model for transaction list page.
    /// </summary>
    public class TransactionListViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the paginated transaction list.
        /// </summary>
        public TransactionListResponseDto Transactions { get; set; } = new();

        /// <summary>
        /// Gets or sets the current filter criteria.
        /// </summary>
        public TransactionFilterDto Filter { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of products for filter dropdown.
        /// </summary>
        public IEnumerable<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();

        /// <summary>
        /// Gets or sets the list of warehouses for filter dropdown.
        /// </summary>
        public IEnumerable<WarehouseResponseDto> Warehouses { get; set; } = new List<WarehouseResponseDto>();
    }
}
