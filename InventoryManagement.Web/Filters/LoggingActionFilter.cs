using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace InventoryManagement.Web.Filters
{
    /// <summary>
    /// Action filter for logging request/response information.
    /// </summary>
    public class LoggingActionFilter : IAsyncActionFilter, IAsyncResultFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;

        /// <summary>
        /// Initializes a new instance of the LoggingActionFilter class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log before action execution
            _logger.LogInformation(
                "Executing action: {Controller}/{Action} with parameters: {@Parameters}",
                context.Controller.GetType().Name,
                context.ActionDescriptor.DisplayName,
                context.ActionArguments);

            var resultContext = await next();

            stopwatch.Stop();

            // Log after action execution
            _logger.LogInformation(
                "Action executed: {Controller}/{Action} - Status: {Status} - Duration: {Duration}ms",
                context.Controller.GetType().Name,
                context.ActionDescriptor.DisplayName,
                resultContext.Exception == null ? "Success" : "Failed",
                stopwatch.ElapsedMilliseconds);
        }

        /// <inheritdoc/>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            _logger.LogInformation(
                "Executing result for: {Controller}/{Action} - Result Type: {ResultType}",
                context.Controller.GetType().Name,
                context.ActionDescriptor.DisplayName,
                context.Result.GetType().Name);

            await next();

            _logger.LogInformation(
                "Result executed for: {Controller}/{Action}",
                context.Controller.GetType().Name,
                context.ActionDescriptor.DisplayName);
        }
    }
}
