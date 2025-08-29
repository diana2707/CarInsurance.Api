namespace CarInsurance.Api.Shared
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
