using System.ComponentModel.DataAnnotations;

namespace DomainModels;

public class Room : Common
{
    public int Beds { get; set; }
    public int KingBeds { get; set; }
    public int QueenBeds { get; set; }
    public int TwinBeds { get; set; }
    public int Size { get; set; }
    public int Tv { get; set; }
    public bool Bathroom { get; set; }
    public bool Clean { get; set; }
    public bool Bathtub { get; set; }
    public bool WiFi { get; set; }
    public bool Fridge { get; set; }
    public bool Stove { get; set; }
    public bool Oven { get; set; }
    public bool Microwave { get; set; }
    public double Price { get; set; }
    public string? Description { get; set; }
    
    public ICollection<BookingsRooms> BookingRooms { get; set; } = new List<BookingsRooms>();
}

public class RoomPostDto
{
    public int Beds { get; set; }
    public int KingBeds { get; set; }
    public int QueenBeds { get; set; }
    public int TwinBeds { get; set; }
    public int Size { get; set; }
    public int Tv { get; set; }
    public bool Bathroom { get; set; }
    public bool Clean { get; set; }
    public bool Bathtub { get; set; }
    public bool WiFi { get; set; }
    public bool Fridge { get; set; }
    public bool Stove { get; set; }
    public bool Oven { get; set; }
    public bool Microwave { get; set; }
    public double Price { get; set; }
    public string? Description { get; set; }
}

public class RoomOnlyDto
{
    public int Beds { get; set; }
    public int KingBeds { get; set; }
    public int QueenBeds { get; set; }
    public int TwinBeds { get; set; }
    public int Size { get; set; }
    public int Tv { get; set; }
    public bool Bathroom { get; set; }
    public bool Clean { get; set; }
    public bool Bathtub { get; set; }
    public bool WiFi { get; set; }
    public bool Fridge { get; set; }
    public bool Stove { get; set; }
    public bool Oven { get; set; }
    public bool Microwave { get; set; }
    public double Price { get; set; }
    public string? Description { get; set; }
}

public class TotalRoomsDetails
{
    public int TotalRooms { get; set; }
    public int TotalBeds { get; set; }
    public int TotalQueenBeds { get; set; }
    public int TotalKingBeds { get; set; }
    public int TotalTwinBeds { get; set; }
    public int TotalCleanRooms { get; set; }
    public int TotalTvs { get; set; }
    public int TotalBathrooms { get; set; }
    public int TotalBathtubs { get; set; }
    public int TotalWifis { get; set; }
    public int TotalFridges { get; set; }
    public int TotalStoves { get; set; }
    public int TotalOvens { get; set; }
    public int TotalMicrowaves { get; set; }
    public double TotalPrice { get; set; }
    public double AvgPrice { get; set; }
    public double HighestPrice { get; set; }
    public double LowestPrice { get; set; }
}

public class RoomOccupation
{
    public string RoomId { get; set; }
    public Dictionary<DateTime, DateTime> OccupiedDates { get; set; } = new();
}