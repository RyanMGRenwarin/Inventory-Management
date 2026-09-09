using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Web.ViewModels.Product
{
    /// <summary>
    /// View model for product delete confirmation.
    /// </summary>
    public class ProductDeleteViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the product to delete.
        /// </summary>
        public ProductResponseDto Product { get; set; } = new();
    }
}
