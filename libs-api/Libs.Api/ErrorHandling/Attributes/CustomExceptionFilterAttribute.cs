using System.Net;
using Libs.Api.ErrorHandling.Exceptions;
using Libs.Api.ErrorHandling.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Libs.Api.ErrorHandling.Attributes;

public class CustomExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var exceptionType = context.Exception.GetType();
        ErrorResponse errorResponse;

        switch (exceptionType.Name)
        {
            case nameof(DomainException):
                errorResponse = new ErrorResponse(HttpStatusCode.BadRequest, context.Exception.Message, exceptionType.Name);
                break;

            default:
                errorResponse = new ErrorResponse(HttpStatusCode.InternalServerError, context.Exception.Message, exceptionType.Name);
                break;
        }

        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = (int)errorResponse.StatusCode
        };

        context.ExceptionHandled = true;
    }
}