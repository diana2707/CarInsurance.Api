using CarInsurance.Api.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarInsurance.Api.Tests.SharedTests.Fakes
{
    public class FakeServiceScopeFactory : IServiceScopeFactory
    {
        private FakeLogger<PolicyExpirationLogger> _logger = new();

        public IServiceScope CreateScope()
        {
            return new FakeServiceScope(_logger);
        }

        private class FakeServiceScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }

            public FakeServiceScope(FakeLogger<PolicyExpirationLogger> logger)
            {
                ServiceProvider = new FakeServiceProvider(logger);
            }

            public void Dispose() { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            ILogger<PolicyExpirationLogger> _logger;
            public FakeServiceProvider(FakeLogger<PolicyExpirationLogger> logger)
            {
                _logger = logger;
            }

            public object? GetService(Type serviceType)
            {
                return _logger;
            }
        }
    }
    
}
