using Libs.Api.ErrorHandling.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Libs.Api.ErrorHandling;
public class ErrorService : IErrorService
{
    private List<ErrorResponse> _errors;
    private HttpStatusCode MajorErrorCode => _errors.Max(x => x.StatusCode);
    private readonly ILogger<ErrorService> _logger;
    public bool HasErrors => _errors.Any();

    public ErrorService(ILogger<ErrorService> logger)
    {
        _errors = new List<ErrorResponse>();
        _logger = logger;
    }

    public async Task HandleErrors(HttpContext context)
    {
        context.Response.StatusCode = (int)MajorErrorCode;
        context.Response.ContentType = "application/json";
        _errors.ForEach(x => _logger.LogInformation(x.Message));
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