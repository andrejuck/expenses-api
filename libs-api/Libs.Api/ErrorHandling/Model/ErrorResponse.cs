
using System.Net;

namespace Libs.Api.ErrorHandling.Model;

public class ErrorResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string Message { get; set; }
    public string Action { get; set; }

}