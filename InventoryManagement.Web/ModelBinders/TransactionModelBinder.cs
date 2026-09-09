using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventoryManagement.Web.ModelBinders
{
    /// <summary>
    /// Custom model binder for TransactionCreateDto to handle complex binding.
    /// </summary>
    public class TransactionModelBinder : IModelBinder
    {
        private readonly ILogger<TransactionModelBinder> _logger;

        /// <summary>
        /// Initializes a new instance of the TransactionModelBinder class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public TransactionModelBinder(ILogger<TransactionModelBinder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            try
            {
                var dto = new TransactionCreateDto();

                // Bind properties using TryParse or direct value
                if (int.TryParse(bindingContext.ValueProvider.GetValue($"{modelName}.ProductId").FirstValue, out int productId))
                    dto.ProductId = productId;

                if (int.TryParse(bindingContext.ValueProvider.GetValue($"{modelName}.WarehouseId").FirstValue, out int warehouseId))
                    dto.WarehouseId = warehouseId;

                if (int.TryParse(bindingContext.ValueProvider.GetValue($"{modelName}.Quantity").FirstValue, out int quantity))
                    dto.Quantity = quantity;

                if (decimal.TryParse(bindingContext.ValueProvider.GetValue($"{modelName}.UnitPrice").FirstValue, out decimal unitPrice))
                    dto.UnitPrice = unitPrice;

                if (Enum.TryParse<TransactionType>(bindingContext.ValueProvider.GetValue($"{modelName}.Type").FirstValue, out TransactionType type))
                    dto.Type = type;

                dto.Notes = bindingContext.ValueProvider.GetValue($"{modelName}.Notes").FirstValue ?? string.Empty;

                // Set the model and mark as success
                bindingContext.Result = ModelBindingResult.Success(dto);
                _logger.LogDebug("Successfully bound TransactionCreateDto: {@Dto}", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error binding TransactionCreateDto");
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
