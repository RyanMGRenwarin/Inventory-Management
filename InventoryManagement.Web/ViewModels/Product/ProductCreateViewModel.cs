using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.DTOs.Product;
using System.ComponentModel;

namespace InventoryManagement.Web.ViewModels.Product
{
    /// <summary>
    /// View model for product create page.
    /// </summary>
    public class ProductCreateViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the product creation data.
        /// </summary>
        public ProductCreateDto Product { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of categories for dropdown.
        /// </summary>
        public IEnumerable<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();

        [DisplayName("Unit Price")]
        public string? PriceRaw { get; set; }
    }
}
