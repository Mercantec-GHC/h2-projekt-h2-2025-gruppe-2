using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DomainModels;

public class Booking : Common
{
    public int Adults { get; set; }
    public int Children { get; set; }
    public bool RoomService { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public double TotalPrice { get; set; }
    public DateTime OccupiedFrom { get; set; }
    public DateTime OccupiedTill { get; set; }
    
    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public ICollection<BookingsRooms> BookingRooms { get; set; } = new List<BookingsRooms>();

    public void CalcTotalPrice(List<int> roomIds)
    {
        TotalPrice = 0;

        if (RoomService)
            TotalPrice += 195;
        if (Breakfast)
            TotalPrice += 100;
        if (Dinner)
            TotalPrice += 295;

        int days = (int)Math.Ceiling((OccupiedTill - OccupiedFrom).TotalDays);
        TotalPrice += days * 495;

        TotalPrice += CalcRoomsPrice(roomIds);
    }

    private double CalcRoomsPrice(List<int> roomIds)
    {
        double price = 0;
        foreach (int roomId in roomIds)
        {
            // Finds all the rooms with by the relevant room id's, which the user has selected,
            // and adds the price of the rooms to the total.
        }

        return price;
    }
}

public class BookingPostDto
{
    public int Adults { get; set; }
    public int Children { get; set; }
    public bool RoomService { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public double TotalPrice { get; set; }
    public List<string> RoomIds { get; set; } = null!;
    public DateTime OccupiedFrom { get; set; }
    public DateTime OccupiedTill { get; set; }
}

public class BookingRoomsDto()
{
    public string BookingId { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public bool RoomService { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public DateTime OccupiedFrom { get; set; }
    public DateTime OccupiedTill { get; set; }
    public List<string> RoomIds { get; set; }
}

public class NewBookingDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Adults must be between 1 and 100")]
    public int Adults { get; set; }
    public int Children { get; set; }
    public bool RoomService { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public double TotalPrice { get; set; }
    public List<string> RoomIds { get; set; } = [];
    public DateTime occupiedFrom { get; set; }
    public DateTime occupiedTill { get; set; }
}

public class BookingDetails
{
    public int TotalBookings { get; set; }
    public int TotalAdults { get; set; }
    public int TotalChildren { get; set; }
    public int TotalBookedRooms { get; set; }
    public int TotalBookedDays { get; set; }
    public int TotalRoomService { get; set; }
    public int TotalBreakfast { get; set; }
    public int TotalDinner { get; set; }
    public int TotalActiveBookings { get; set; }
    public int TotalActiveBookedRooms { get; set; }
    public int TotalInactiveBookedRooms { get; set; }
    public int TotalInactiveBookings { get; set; }
    public double TotalBookingsPrice { get; set; }
    public double AvgPrice { get; set; }
    public double HighestPrice { get; set; }
    public double LowestPrice { get; set; }
}