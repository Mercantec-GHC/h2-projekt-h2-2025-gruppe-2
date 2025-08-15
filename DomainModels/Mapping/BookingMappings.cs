namespace DomainModels.Mapping;

public class BookingMappings
{
    public static BookingPostDto ToBookingPostDto(Booking booking)
    {
        return new BookingPostDto
        {
            Id = booking.Id,
            Adults = booking.Adults,
            Children = booking.Children,
            RoomService = booking.RoomService,
            Breakfast = booking.Breakfast,
            Dinner = booking.Dinner,
            UserId = booking.UserId,
            OccupiedFrom = booking.OccupiedFrom,
            OccupiedTill = booking.OccupiedTill,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt,
        };
    }
}