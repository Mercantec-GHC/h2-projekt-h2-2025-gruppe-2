namespace DomainModels;

public class Admin : CommonRoles
{
    public required Common CommonLogins { get; set; }
    public void BookRoom(int userId)
    {
        
    }

    public void ResetPassword(int userId)
    {
        
    }

    public void UpdateProfile(int userId)
    {
        
    }

    public void DeleteProfile(int userId)
    {
        
    }
}