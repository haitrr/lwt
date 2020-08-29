namespace Lwt
{
    using System.Threading.Tasks;
    using Lwt.Models;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class LanguageCodeModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                string? modelTypeValue = bindingContext.ValueProvider.GetValue(nameof(LanguageCode)).FirstValue;

                try
                {
                    LanguageCode val = LanguageCode.GetFromString(modelTypeValue);
                    bindingContext.Result = ModelBindingResult.Success(val);
                }

                #pragma warning disable
                catch
                #pragma warning restore
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                }

                return Task.CompletedTask;
            }
        }
}