using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace CarInsurance.Api.Services
{
    public class PolicyExpirationLogger : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PolicyExpirationLogger> _logger;
        private readonly TimeSpan intervalCheck = TimeSpan.FromMinutes(10);
        private readonly List<InsurancePolicy> expiredPolicies = new();

        public PolicyExpirationLogger(IServiceScopeFactory scopeFactory, ILogger<PolicyExpirationLogger> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var policies = await db.Policies.Where(p => p.EndDate.ToDateTime(TimeOnly.MinValue) < DateTime.Now
                                           && p.EndDate.ToDateTime(TimeOnly.MinValue) > DateTime.Now.AddHours(-1)
                                           && !expiredPolicies.Contains(p))
                                           .ToListAsync();

                expiredPolicies.AddRange(policies);

                foreach (var p in policies){
                    _logger.LogInformation($"Policy with ID {p.Id} for Car ID {p.CarId} expired on {p.EndDate}.");
                }

                await Task.Delay(intervalCheck, stoppingToken);
            }
        }
    }
}
