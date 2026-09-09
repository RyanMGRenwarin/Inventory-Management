using InventoryManagement.Application.DTOs.Product;
using InventoryManagement.Application.DTOs.Transaction;

namespace InventoryManagement.Web.ViewModels
{
    /// <summary>
    /// View model for dashboard page.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the transaction summary.
        /// </summary>
        public TransactionSummaryDto TransactionSummary { get; set; } = new();

        /// <summary>
        /// Gets or sets the low stock products.
        /// </summary>
        public IEnumerable<ProductResponseDto> LowStockProducts { get; set; } = new List<ProductResponseDto>();

        /// <summary>
        /// Gets or sets the total number of products.
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Gets or sets the total number of warehouses.
        /// </summary>
        public int TotalWarehouses { get; set; }

        /// <summary>
        /// Gets or sets the total number of categories.
        /// </summary>
        public int TotalCategories { get; set; }
    }
}
