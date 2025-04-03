using Libs.Api.ErrorHandling.Model;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Libs.Api.ErrorHandling;

public interface IErrorService
{
    public bool HasErrors { get; }
    Task HandleErrors(HttpContext context);
    void AddError(ErrorResponse error);
    void AddError(string action, string message, HttpStatusCode statusCode);
}