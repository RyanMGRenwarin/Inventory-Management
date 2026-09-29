using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;

namespace InventoryManagement.Web.Filters
{
    /// <summary>
    /// Action filter for automatic model validation.
    /// </summary>
    public class ValidationActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationActionFilter> _logger;

        public ValidationActionFilter(ILogger<ValidationActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip for GET
            if (context.HttpContext.Request.Method == "GET")
            {
                await next();
                return;
            }

            // Binding decimal before validation
            FixDecimalBinding(context);

            if (!context.ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for {Action}", context.ActionDescriptor.DisplayName);

                // Return ViewModel (not ModelState)
                var viewModel = context.ActionArguments.Values.FirstOrDefault(
                    v => v != null && v.GetType().Name.EndsWith("ViewModel"));

                if (viewModel != null && context.Controller is Controller controller)
                {
                    context.Result = controller.View(viewModel);
                    return;
                }

                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            await next();
        }

        /// <summary>
        /// Decimal binding for Indonesia Culture (dot/comma).
        /// </summary>
        private void FixDecimalBinding(ActionExecutingContext context)
        {
            var form = context.HttpContext.Request.Form;
            var culture = new CultureInfo("id-ID");

            foreach (var key in form.Keys)
            {
                var value = form[key].ToString();

                // Check whether or not this field is decimal field (based on name or type)
                if (IsDecimalField(key, context))
                {
                    var normalized = value.Replace(".", ",");
                    if (decimal.TryParse(normalized, NumberStyles.Any, culture, out decimal result))
                    {
                        // Update model state parsed value
                        context.ModelState.SetModelValue(key, value, normalized);
                        // Update argument value
                        UpdateArgumentValue(context, key, result);
                    }
                }
            }
        }

        private bool IsDecimalField(string key, ActionExecutingContext context)
        {
            // Check whether or not this field is decimal field (based on name or type)
            var modelState = context.ModelState[key];
            if (modelState != null)
            {
                var rawValue = modelState.RawValue;
                if (rawValue != null && rawValue.GetType() == typeof(decimal))
                    return true;
            }

            // Or based on field name (example: "Price", "UnitPrice", dll)
            var lowerKey = key.ToLower();
            return lowerKey.Contains("price") || lowerKey.Contains("amount") || lowerKey.Contains("total");
        }

        private void UpdateArgumentValue(ActionExecutingContext context, string key, decimal value)
        {
            // Search argument that has property with key name
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;

                var prop = arg.GetType().GetProperty(key);
                if (prop != null && prop.PropertyType == typeof(decimal))
                {
                    prop.SetValue(arg, value);
                }
            }
        }
    }
}
