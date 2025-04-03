using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Libs.Api.ErrorHandling.Attributes;

public class ErrorFilterAttribute : Attribute, IAsyncResultFilter
{
    public virtual async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var errorService = context.HttpContext.RequestServices.GetService<IErrorService>();
        if (errorService.HasErrors)
        {
            await errorService.HandleErrors(context.HttpContext);
            return;
        }

        await next();
    }
}