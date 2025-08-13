namespace DomainModels;

public class BookingsRooms : Common
{
    public string BookingId { get; set; } = null!;
    public Booking Booking { get; set; } = null!;

    public string RoomId { get; set; } = null!;
    public Room Room { get; set; } = null!;
}