namespace API.Service;

/// <summary>
/// Service for retrieving the current time in the Copenhagen time zone (UTC+2).
/// </summary>
public class TimeService
{
    /// <summary>
    /// Gets the current date and time in the Copenhagen time zone (UTC+2).
    /// </summary>
    /// <returns>The current <see cref="DateTime"/> in UTC+2.</returns>
    public DateTime GetCopenhagenTime()
    {
        DateTime utcNow = DateTime.UtcNow.AddHours(2);
        return utcNow;
    }
}