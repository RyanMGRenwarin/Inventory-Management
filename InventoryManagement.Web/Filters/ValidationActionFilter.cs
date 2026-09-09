using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InventoryManagement.Web.Filters
{
    /// <summary>
    /// Action filter for automatic model validation.
    /// </summary>
    public class ValidationActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationActionFilter> _logger;

        /// <summary>
        /// Initializes a new instance of the ValidationActionFilter class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public ValidationActionFilter(ILogger<ValidationActionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Skip for GET
            if (context.HttpContext.Request.Method == "GET")
            {
                await next();
                return;
            }

            if (!context.ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed for {Action}", context.ActionDescriptor.DisplayName);

                // Take View Model from Argument
                var viewModel = context.ActionArguments.Values.FirstOrDefault(
                    v => v != null && v.GetType().Name.EndsWith("ViewModel"));

                if (viewModel != null && context.Controller is Controller controller)
                {
                    // Return View With The Same View Model
                    context.Result = controller.View(viewModel);
                    return;
                }

                // Fallback if there is not view model
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            await next();
        }
    }
}
