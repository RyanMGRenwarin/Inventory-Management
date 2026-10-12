using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Web.ViewModels.Transaction;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace InventoryManagement.Web.ModelBinders
{
    /// <summary>
    /// Custom model binder for TransactionCreateViewModel to handle complex binding
    /// including decimal parsing with Indonesian culture (dot/comma).
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
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            try
            {
                var viewModel = new TransactionCreateViewModel();

                // Bind Transaction.ProductId
                if (int.TryParse(
                    bindingContext.ValueProvider.GetValue("Transaction.ProductId").FirstValue,
                    out int productId))
                {
                    viewModel.Transaction.ProductId = productId;
                }

                // Bind Transaction.WarehouseId
                if (int.TryParse(
                    bindingContext.ValueProvider.GetValue("Transaction.WarehouseId").FirstValue,
                    out int warehouseId))
                {
                    viewModel.Transaction.WarehouseId = warehouseId;
                }

                // Bind Transaction.Type
                if (Enum.TryParse<TransactionType>(
                    bindingContext.ValueProvider.GetValue("Transaction.Type").FirstValue,
                    out TransactionType type))
                {
                    viewModel.Transaction.Type = type;
                }

                // Bind Transaction.Quantity
                if (int.TryParse(
                    bindingContext.ValueProvider.GetValue("Transaction.Quantity").FirstValue,
                    out int quantity))
                {
                    viewModel.Transaction.Quantity = quantity;
                }

                // Bind UnitPriceRaw (string) — decimal parsing handled here
                var unitPriceRaw = bindingContext.ValueProvider
                    .GetValue("UnitPriceRaw").FirstValue;
                viewModel.UnitPriceRaw = unitPriceRaw;

                if (!string.IsNullOrWhiteSpace(unitPriceRaw))
                {
                    viewModel.Transaction.UnitPrice = ParseDecimal(unitPriceRaw);
                }

                // Bind Transaction.Notes
                viewModel.Transaction.Notes = bindingContext.ValueProvider
                    .GetValue("Transaction.Notes").FirstValue ?? string.Empty;

                // Set the model and mark as success
                bindingContext.Result = ModelBindingResult.Success(viewModel);
                _logger.LogDebug("Successfully bound TransactionCreateViewModel: {@ViewModel}", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error binding TransactionCreateViewModel");
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Parses a decimal string with support for both dot (.) and comma (,) as decimal separators.
        /// </summary>
        /// <param name="value">The raw string value from the form.</param>
        /// <returns>The parsed decimal value, or 0 if parsing fails.</returns>
        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            // Only keep digits, dot, and comma
            var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            // Normalize: dot -> comma (for id-ID culture)
            var normalized = cleaned.Replace(".", ",");

            // Try parse with Indonesian culture (comma as decimal separator)
            if (decimal.TryParse(normalized, NumberStyles.Any, new CultureInfo("id-ID"), out var result))
                return result;

            // Fallback: try parse with invariant culture (dot as decimal separator)
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }
    }
}
