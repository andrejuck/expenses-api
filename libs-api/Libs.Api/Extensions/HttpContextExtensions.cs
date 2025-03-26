using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Libs.Api.Extensions;

public static class HttpContextExtensions
{
    public static CultureInfo GetCultureFromHeader(this HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
        {
            var cultureName = acceptLanguage.ToString().Split(',').FirstOrDefault();

            if (!string.IsNullOrEmpty(cultureName))
            {
                try
                {
                    return new CultureInfo(cultureName);
                }
                catch (CultureNotFoundException)
                {
                    return CultureInfo.InvariantCulture;
                }
            }
        }

        return CultureInfo.InvariantCulture;
    }
}