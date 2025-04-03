using Microsoft.Extensions.Logging;

namespace Libs.Api.Logging
{
    public class CustomLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomLogger(categoryName);
        }

        public void Dispose() { }
    }
}
