using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.DTOs.Product;
using System.ComponentModel;

namespace InventoryManagement.Web.ViewModels.Product
{
    /// <summary>
    /// View model for product edit page.
    /// </summary>
    public class ProductEditViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the product update data.
        /// </summary>
        public ProductUpdateDto Product { get; set; } = new();

        /// <summary
        /// Gets or sets the list of categories for dropdown.
        /// </summary>
        public IEnumerable<CategoryResponseDto> Categories { get; set; } = [];

        [DisplayName("Price")]
        public string PriceRaw { get; set; } = "0";
    }
}
