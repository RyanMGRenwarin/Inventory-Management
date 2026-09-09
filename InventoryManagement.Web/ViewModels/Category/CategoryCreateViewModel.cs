using InventoryManagement.Application.DTOs.Category;

namespace InventoryManagement.Web.ViewModels.Category
{
    /// <summary>
    /// View model for category create page.
    /// </summary>
    public class CategoryCreateViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the category creation data.
        /// </summary>
        public CategoryCreateDto Category { get; set; } = new();
    }
}
