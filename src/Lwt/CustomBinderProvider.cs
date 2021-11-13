namespace Lwt;

using Lwt.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public class CustomBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType != typeof(LanguageCode))
        {
            return null!;
        }

        return new LanguageCodeModelBinder();
    }
}