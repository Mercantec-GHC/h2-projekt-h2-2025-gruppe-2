namespace DomainModels.Mapping;

public static class BookingMappings
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

    public static List<BookingRoomsDto> ToBookingRoomsDto(List<Booking> bookingsRooms)
    {
        List<BookingRoomsDto> bookingsRoomsDtos = new List<BookingRoomsDto>();
        foreach (Booking booking in bookingsRooms)
        {
            var tempRoomId = new List<string>();
            var tempBook = new BookingRoomsDto
            {
                BookingId = booking.Id,
                Adults = booking.Adults,
                Children = booking.Children,
                Breakfast = booking.Breakfast,
                Dinner = booking.Dinner,
                RoomService = booking.RoomService,
                OccupiedFrom = booking.OccupiedFrom,
                OccupiedTill = booking.OccupiedTill,
            };
            
            foreach (BookingsRooms bookingBooking in booking.BookingRooms)
            {
                tempRoomId.Add(bookingBooking.RoomId);
            }
            
            tempBook.RoomIds = tempRoomId;
            bookingsRoomsDtos.Add(tempBook);
        }

        return bookingsRoomsDtos;
    }
}