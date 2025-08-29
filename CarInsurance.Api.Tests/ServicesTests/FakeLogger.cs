using Microsoft.Extensions.Logging;

namespace CarInsurance.Api.Tests.ServicesTests
{
    public class FakeLogger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        public List<string> Messages { get; } = new List<string>();

        public void LogInformation(string? message)
        {
            Messages.Add(message ?? string.Empty);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => new StubDisposable();

        private class StubDisposable() : IDisposable

        {
            public void Dispose() { }
        }
    }

   
}
