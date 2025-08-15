namespace API.Service;

public class TimeService
{
    
    public DateTime GetCopenhagenTime()
    {
        DateTime utcNow = DateTime.UtcNow.AddHours(2);
        return utcNow;
    }
}