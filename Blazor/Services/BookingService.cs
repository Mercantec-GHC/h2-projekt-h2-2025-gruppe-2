namespace Blazor.Services;

public class BookingService
{
    public double ROOM_SERVICE_PRICE = 245;
    public double BREAKFAST_PRICE = 195;
    public double DINNER_PRICE = 395;
    IConfiguration _configuration;

    public BookingService(IConfiguration configuration)
    {
        _configuration = configuration;
        ROOM_SERVICE_PRICE =
            double.TryParse(configuration["prices:ROOM_SERVICE"] ??
                            Environment.GetEnvironmentVariable("ROOM_SERVICE"), out var result)
                ? result
                :241.0;
        
        BREAKFAST_PRICE =
            double.TryParse(configuration["prices:BREAKFAST"] ??
                            Environment.GetEnvironmentVariable("BREAKFAST"), out var result2)
                ? result2
                :191.0;
        
        DINNER_PRICE =
            double.TryParse(configuration["prices:DINNER"] ??
                            Environment.GetEnvironmentVariable("DINNER"), out var result3)
                ? result3
                :391.0;
    }
}