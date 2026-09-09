using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.DTOs.Warehouse;

namespace InventoryManagement.Web.ViewModels.Transaction
{
    /// <summary>
    /// View model for transaction create page.
    /// </summary>
    public class TransactionCreateViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the transaction creation data.
        /// </summary>
        public TransactionCreateDto Transaction { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of products for dropdown.
        /// </summary>
        public IEnumerable<ProductResponseDto> Products { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of warehouses for dropdown.
        /// </summary>
        public IEnumerable<WarehouseResponseDto> Warehouses { get; set; } = [];
    }
}
