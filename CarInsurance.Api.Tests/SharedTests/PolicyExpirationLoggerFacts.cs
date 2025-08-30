using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Shared;
using CarInsurance.Api.Tests.SharedTests.Fakes;
using Microsoft.EntityFrameworkCore;


namespace CarInsurance.Api.Tests.SharedTests
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
        public async Task RunOnce_LogsExpiringPolicies_PoliciesExist()
        {
            FakeDateTimeProvider nowPovider = new();
            nowPovider.UtcNow = new DateTime(2025, 8, 29, 0, 0, 1);

            var policy = new InsurancePolicy
            {
                CarId = 1,
                Provider = "Test",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 8, 29)
            };
            _db.Policies.Add(policy);
            _db.SaveChanges();

            string expectedMsg = $"Policy with ID {policy.Id} for Car ID {policy.CarId} expired on {policy.EndDate}.";

            var policyLoger = new PolicyExpirationLogger(_fakeScopeFactory, _fakeLogger, nowPovider);
            await policyLoger.RunOnce(_db, CancellationToken.None);

            Assert.NotNull(_fakeLogger);
            Assert.Equal(expectedMsg, _fakeLogger.Messages.First());
        }

        [Fact]
        public async Task RunOnce_LogsExpiringPolicies_NoPolicieExist()
        {
            FakeDateTimeProvider nowPovider = new();
            nowPovider.UtcNow = new DateTime(2025, 8, 29, 0, 0, 1);

            var policyLoger = new PolicyExpirationLogger(_fakeScopeFactory, _fakeLogger, nowPovider);
            await policyLoger.RunOnce(_db, CancellationToken.None);

            Assert.NotNull(_fakeLogger);
            Assert.Empty(_fakeLogger.Messages);
        }

        [Fact]
        public async Task RunOnce_LogsExpiringPolicies_LogMultiplePolicies()
        {
            FakeDateTimeProvider nowPovider = new();
            nowPovider.UtcNow = new DateTime(2025, 8, 29, 0, 0, 1);

            _db.Policies.AddRange(
                new InsurancePolicy { CarId = 1, Provider = "A", StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2025, 8, 29) },
                new InsurancePolicy { CarId = 2, Provider = "B", StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2025, 8, 29) });
            _db.SaveChanges();

            var policyLoger = new PolicyExpirationLogger(_fakeScopeFactory, _fakeLogger, nowPovider);

            await policyLoger.RunOnce(_db, CancellationToken.None);

            Assert.Equal(2, _fakeLogger.Messages.Count);
        }

        [Fact]
        public async Task RunOnce_LogsExpiringPolicies_LoggedOneHourAgo()
        {
            FakeDateTimeProvider nowPovider = new();
            nowPovider.UtcNow = new DateTime(2025, 8, 29, 1, 0, 0);

            var policy = new InsurancePolicy
            {
                CarId = 1,
                Provider = "Test",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 8, 29)
            };
            _db.Policies.Add(policy);
            _db.SaveChanges();

            var policyLoger = new PolicyExpirationLogger(_fakeScopeFactory, _fakeLogger, nowPovider);

            await policyLoger.RunOnce(_db, CancellationToken.None);

            Assert.Single(_fakeLogger.Messages);
        }

        [Fact]
        public async Task RunOnce_LogsExpiringPolicies_LoggedNow()
        {
            FakeDateTimeProvider nowPovider = new();
            nowPovider.UtcNow = new DateTime(2025, 8, 29, 0, 0, 0);

            var policy = new InsurancePolicy
            {
                CarId = 1,
                Provider = "Test",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 8, 29)
            };
            _db.Policies.Add(policy);
            _db.SaveChanges();

            var policyLoger = new PolicyExpirationLogger(_fakeScopeFactory, _fakeLogger, nowPovider);

            await policyLoger.RunOnce(_db, CancellationToken.None);

            Assert.Single(_fakeLogger.Messages);
        }

        public void Dispose()
        {
            _db.Database.CloseConnection();
            _db.Dispose();
        }
    }
}
