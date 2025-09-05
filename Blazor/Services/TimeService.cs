namespace Blazor.Services;

public class TimeService
{
    public DateTime TranslateToUtc(DateOnly date)
    {
        return DateTime.SpecifyKind(
            date.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc
        ).AddHours(2);
    }
}