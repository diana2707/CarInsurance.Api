using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
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
        private readonly DateTime _now;

        public PolicyExpirationLogger(IServiceScopeFactory scopeFactory, ILogger<PolicyExpirationLogger> logger, DateTime now)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _now = now;
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
            var expiredPolicies = await db.Policies.Where(p => p.EndDate.ToDateTime(TimeOnly.MinValue) < _now
                                           && p.EndDate.ToDateTime(TimeOnly.MinValue) > _now.AddHours(-1)
                                           && !loggedPolicies.Contains(p.Id))
                                           .ToListAsync();

            loggedPolicies.AddRange(expiredPolicies.Select(p => p.Id));

            foreach (var p in expiredPolicies)
            {
                _logger.LogInformation($"Policy with ID {p.Id} for Car ID {p.CarId} expired on {p.EndDate}.");
            }
        }
    }
}
