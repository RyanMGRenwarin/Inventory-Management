using InventoryManagement.Application.DTOs.Category;

namespace InventoryManagement.Web.ViewModels.Category
{
    /// <summary>
    /// View model for category list page.
    /// </summary>
    public class CategoryListViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the list of categories.
        /// </summary>
        public IEnumerable<CategoryResponseDto> Categories { get; set; } = [];

        /// <summary>
        /// Gets or sets the current filter criteria.
        /// </summary>
        public CategoryFilterDto Filter { get; set; } = new();
    }
}
