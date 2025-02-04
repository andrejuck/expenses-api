using System.Net;
using System.Text.Json;
using Libs.Api.ErrorHandling.Model;
using Microsoft.AspNetCore.Http;

namespace Libs.Api.ErrorHandling;
public class ErrorService : IErrorService
{
    private List<ErrorResponse> _errors;
    private HttpStatusCode MajorErrorCode => _errors.Max(x => x.StatusCode);
    public bool HasErrors => _errors.Any();

    public ErrorService()
    {
        _errors = new List<ErrorResponse>();
    }

    public async Task HandleErrors(HttpContext context)
    {
        context.Response.StatusCode = (int)MajorErrorCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(_errors));
    }

    public void AddError(ErrorResponse error)
    {
        _errors.Add(error);
    }

    public void AddError(string action, string message, HttpStatusCode statusCode)
    {
        _errors.Add(new ErrorResponse(statusCode, message, action));
    }
}