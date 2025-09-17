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
    
    public string MakeDateReadable(DateOnly date)
    {
        // Get the day with the ordinal suffix
        int day = date.Day;
        string daySuffix = day switch
        {
            1 or 21 or 31 => "st",
            2 or 22 => "nd",
            3 or 23 => "rd",
            _ => "th"
        };

        // Get the month name
        string monthName = date.ToString("MMMM");

        // Build the readable date string
        return $"{day}{daySuffix} of {monthName}";
    }
}