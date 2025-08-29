using CarInsurance.Api.Data;
using CarInsurance.Api.Shared;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("CarInsurance.Api.Tests")]

namespace CarInsurance.Api.Services
{
    public class PolicyExpirationLogger : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PolicyExpirationLogger> _logger;
        private readonly TimeSpan intervalCheck = TimeSpan.FromMinutes(10);
        private readonly List<long> loggedPolicies = new();
        private readonly IDateTimeProvider _nowProvider;

        public PolicyExpirationLogger(IServiceScopeFactory scopeFactory, ILogger<PolicyExpirationLogger> logger, IDateTimeProvider nowProvider)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _nowProvider = nowProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await RunOnce(stoppingToken, db);


                await Task.Delay(intervalCheck, stoppingToken);
            }
        }

        internal async Task RunOnce(CancellationToken stoppingToken, AppDbContext db)
        {
            DateTime now = _nowProvider.UtcNow;
            DateTime oneHourAgo = now.AddHours(-1);

            var candidatePolicies = await db.Policies
                .Where(p => p.EndDate <= DateOnly.FromDateTime(now))
                .ToListAsync(stoppingToken);

            var expiredPolicies = candidatePolicies
                .Where(p => p.EndDate.ToDateTime(TimeOnly.MinValue) <= now
                         && p.EndDate.ToDateTime(TimeOnly.MinValue) >= oneHourAgo
                         && !loggedPolicies.Contains(p.Id))
                .ToList();

            loggedPolicies.AddRange(expiredPolicies.Select(p => p.Id));

            foreach (var p in expiredPolicies)
            {
                _logger.LogInformation($"Policy with ID {p.Id} for Car ID {p.CarId} expired on {p.EndDate}.");
            }
        }
    }
}
