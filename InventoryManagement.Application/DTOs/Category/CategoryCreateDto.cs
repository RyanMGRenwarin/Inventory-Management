using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs.Category
{
    /// <summary>
    /// DTO for creating a new category.
    /// </summary>
    public class CategoryCreateDto
    {
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters")]
        [DisplayName("Category Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [DisplayName("Description")]
        public string Description { get; set; } = string.Empty;
    }
}
