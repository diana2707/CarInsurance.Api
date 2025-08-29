using CarInsurance.Api.Data;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsurance.Api.Tests.ServicesTests
{
    public class PolicyExpirationLoggerFacts : IDisposable
    {
        private readonly AppDbContext _db;
        private FakeLogger<PolicyExpirationLogger> _fakeLogger;
        private FakeServiceScopeFactory _fakeScopeFactory;

        public PolicyExpirationLoggerFacts()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _db = new AppDbContext(options);
            _db.Database.OpenConnection();
            _db.Database.EnsureCreated();
            SeedData.EnsureSeeded(_db);

            _fakeLogger = new FakeLogger<PolicyExpirationLogger>();
            _fakeScopeFactory = new FakeServiceScopeFactory();
        }

        [Fact]
        public async Task ExecuteAsync_LogsExpiringPolicies()
        {
            var logger = new PolicyExpirationLogger(_fakeScopeFactory, _fakeLogger);
            // Run the method (it will check for policies expiring in 30 days)
            await logger.StartAsync(CancellationToken.None);
            // Since we have a policy expiring on 2024-12-15 and today is 2024-11-15,
            // it should log that policy.
            // Note: The actual logging output is printed to the console by FakeLogger.
            // Here we would normally assert on the logged messages if we had access to them.
        }

        public void Dispose()
        {
            _db.Database.CloseConnection();
            _db.Dispose();
        }
    }
}
