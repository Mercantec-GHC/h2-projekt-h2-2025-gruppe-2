namespace DomainModels;

public class CleaningStaff
{
    public required Common CommonLogins { get; set; }
    
    public void CleanRoom(int roomId)
    {
        
    }

    public List<Room> GetUncleanRooms()
    {
        return [];
    }
}