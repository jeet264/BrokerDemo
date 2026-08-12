using BrokerOS.Application.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BrokerOS.Api.Filters;

public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null or CancellationToken or HttpContext or HttpRequest or HttpResponse)
            {
                continue;
            }

            var argumentType = argument.GetType();
            if (argumentType.IsPrimitive || argument is string)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid)
            {
                continue;
            }

            var errors = result.Errors
                .Select(error => new ApiError
                {
                    Field = ApiErrorMapper.ToCamelCase(error.PropertyName),
                    Message = error.ErrorMessage
                })
                .ToArray();

            context.Result = new BadRequestObjectResult(
                ApiResponse.Fail("Validation failed", errors, context.HttpContext.TraceIdentifier));
            return;
        }

        await next();
    }
}
