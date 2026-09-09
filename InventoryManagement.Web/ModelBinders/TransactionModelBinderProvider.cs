using InventoryManagement.Application.DTOs.Transaction;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace InventoryManagement.Web.ModelBinders
{
    /// <summary>
    /// Model binder provider for TransactionModelBinder.
    /// </summary>
    public class TransactionModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc/>
        IModelBinder? IModelBinderProvider.GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(TransactionCreateDto))
            {
                return new BinderTypeModelBinder(typeof(TransactionModelBinder));
            }

            return null;
        }
    }
}
