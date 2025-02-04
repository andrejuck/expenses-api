
using System.Net;

namespace Libs.Api.ErrorHandling.Model;

public class ErrorResponse
{
    public ErrorResponse(HttpStatusCode statusCode, string message, string action)
    {
        StatusCode = statusCode;
        Message = message;
        Action = action;
    }

    public HttpStatusCode StatusCode { get; private set; }
    public string Message { get; private set; }
    public string Action { get; private set; }

}