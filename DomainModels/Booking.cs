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
    public string Id { get; set; } = null!;
    public int Adults { get; set; }
    public int Children { get; set; }
    public bool RoomService { get; set; }
    public bool Breakfast { get; set; }
    public bool Dinner { get; set; }
    public string UserId { get; set; } = null!;
    public List<string> RoomIds { get; set; } = null!;
    public DateTime OccupiedFrom { get; set; }
    public DateTime OccupiedTill { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
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