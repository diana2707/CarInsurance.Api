using CarInsurance.Api.Shared;

namespace CarInsurance.Api.Tests.SharedTests.Fakes
{
    public class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; } = new DateTime(2025, 1, 1);
    }
}
