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
    public bool WiFi { get; set; }
    public bool Fridge { get; set; }
    public bool Stove { get; set; }
    public bool Oven { get; set; }
    public bool Microwave { get; set; }
    public double Price { get; set; }
    public string? Description { get; set; }
    
}