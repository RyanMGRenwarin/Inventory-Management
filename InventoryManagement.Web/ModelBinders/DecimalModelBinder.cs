using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace InventoryManagement.Web.ModelBinders
{
    public class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            // Try to parse with Culture "id-ID" (use comma as decimal)
            if (decimal.TryParse(value, NumberStyles.Any, new CultureInfo("id-ID"), out decimal result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            // Fallback: Parse with Culture "en-US" (use dot as decimal)
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultInvariant))
            {
                bindingContext.Result = ModelBindingResult.Success(resultInvariant);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid decimal format.");
            return Task.CompletedTask;
        }
    }
}
