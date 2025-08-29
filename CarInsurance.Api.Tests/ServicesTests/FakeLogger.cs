using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsurance.Api.Tests.ServicesTests
{
    public class FakeLogger<T> : ILogger<T>
    {
        private List<string> messages = new List<string>();

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            messages.Add(formatter(state, exception));
            Console.WriteLine(formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) => new StubDisposable();

        private class StubDisposable() : IDisposable

        {
            public void Dispose() { }
        }
    }

   
}
