using Microsoft.Extensions.Logging;

namespace Libs.Api.Logging
{
    public class CustomLogger : ILogger
    {
        private readonly string _categoryName;

        public CustomLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var logMessage = $"[{DateTime.UtcNow:HH:mm:ss.fff}] [{logLevel}]: {formatter(state, exception)} | {_categoryName}";

            if (exception != null)
            {
                logMessage += $" | Exception: {exception.Message} | StackTrace: {exception.StackTrace}";
            }

            Console.WriteLine(logMessage);
        }
    }

}
