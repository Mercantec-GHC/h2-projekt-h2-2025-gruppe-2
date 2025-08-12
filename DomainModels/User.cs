using System.Security.Cryptography;
using System.Text;

namespace DomainModels;

public class User : Common
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string HashedPassword { get; set; }
    public string? Salt {  get; set; }
    public DateTime LastLogin { get; set; }
    public string PasswordBackdoor { get; set; } = string.Empty;
    
    public string RoleId { get; set; } = string.Empty;

    public virtual Role? Role { get; set; }

    /*public void BookRoom()
    {

    }

    public void ResetOwnPassword()
    {

    }

    public void EditOwnProfile()
    {

    }

    public void UpdateOwnProfile()
    {

    }

    public void DeleteOwnBooking()
    {

    }

    public void DeleteOwnProfile()
    {

    }*/

}