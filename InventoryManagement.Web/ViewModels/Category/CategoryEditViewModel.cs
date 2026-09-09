using InventoryManagement.Application.DTOs.Category;

namespace InventoryManagement.Web.ViewModels.Category
{
    /// <summary>
    /// View model for category edit page.
    /// </summary>
    public class CategoryEditViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the category update data.
        /// </summary>
        public CategoryCreateDto Category { get; set; } = new();
    }
}
