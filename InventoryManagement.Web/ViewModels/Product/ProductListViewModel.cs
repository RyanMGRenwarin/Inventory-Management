using InventoryManagement.Application.DTOs.Category;
using InventoryManagement.Application.DTOs.Product;

namespace InventoryManagement.Web.ViewModels.Product
{
    /// <summary>
    /// View model for product list page.
    /// </summary>
    public class ProductListViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the paginated product list.
        /// </summary>
        public ProductListResponseDto Products { get; set; } = new();

        /// <summary>
        /// Gets or sets the current filter criteria.
        /// </summary>
        public ProductFilterDto Filter { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of categories for the filter dropdown.
        /// </summary>
        public IEnumerable<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();
    }
}
