namespace DomainModels.Mapping;

public class RoomMapping
{
    public static RoomPostDto ToRoomGetDto(Room room)
    {
        return new RoomPostDto
        {
            Beds = room.Beds,
            WiFi = room.WiFi,
            Bathroom = room.Bathroom,
            Bathtub = room.Bathtub,
            Clean = room.Clean,
            KingBeds = room.KingBeds,
            QueenBeds = room.QueenBeds,
            Size = room.Size,
            Tv = room.Tv,
            TwinBeds = room.TwinBeds,
            Fridge = room.Fridge,
            Description = room.Description,
            Microwave = room.Microwave,
            Oven = room.Oven,
            Price = room.Price,
            Stove = room.Stove
        };
    }
}