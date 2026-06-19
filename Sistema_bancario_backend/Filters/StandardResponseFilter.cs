using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sistema_bancario_backend.Filters;

public sealed class StandardResponseFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if (executedContext.Result is not ObjectResult objectResult)
            return;

        if (objectResult.Value is not IResultResponse apiResponse)
            return;

        executedContext.Result = new ObjectResult(objectResult.Value)
        {
            StatusCode = (int)apiResponse.StatusCode
        };
    }
}
