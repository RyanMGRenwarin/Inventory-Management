using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using InventoryManagement.Application.Validators;

namespace InventoryManagement.Application.DTOs.Product
{
    /// <summary>
    /// DTO for creating a new product.
    /// </summary>
    public class ProductCreateDto
    {
        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        [DisplayName("Product Name")]
        public string Name { get; set; } = string.Empty;  

        /// <summary>
        /// Gets or sets the SKU.
        /// </summary>
        [Required(ErrorMessage = "SKU is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 20 characters")]
        [DisplayName("SKU")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "SKU can only contain uppercase letters, numbers, and hyphens")]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DisplayName("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit price.
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [DataType(DataType.Currency)]
        [DisplayName("Unit Price")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the initial stock quantity.
        /// </summary>
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        [DisplayName("Stock Quantity")]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the minimum stock threshold.
        /// </summary>
        [Required(ErrorMessage = "Minimum stock threshold is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock threshold cannot be negative")]
        [DisplayName("Minimum Stock Threshold")]
        public int MinimumStockThreshold { get; set; }

        /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
        //[Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
    }
}
