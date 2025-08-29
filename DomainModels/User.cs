using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace DomainModels;

public class User : Common
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string HashedPassword { get; set; }
    public string? Salt { get; set; }
    public DateTime LastLogin { get; set; }
    public string PasswordBackdoor { get; set; } = string.Empty;

    public string RoleId { get; set; } = string.Empty;
    public virtual Role? Roles { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public void SetLoginDto()
    {
        
    }

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

public class RegisterDto
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage =
            "Password must contain at least one number, one uppercase letter, one lowercase letter, and one special character")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage =
            "Password must contain at least one number, one uppercase letter, one lowercase letter, and one special character")]
    public string Password { get; set; } = string.Empty;
}

public class UserGetDto
{
    public string Id { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

public class UserPutDto
{
    public string Id { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
}

public class ChangeOwnPasswordDto
{
    public string CurrentPassword { get; set; } = default!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
ErrorMessage =
    "Password must contain at least one number, one uppercase letter, one lowercase letter, and one special character")]
    public string NewPassword { get; set; } = default!;
}
public class LoginResponseDto
{
    public string message { get; set; }
    public string token { get; set; }
    public UserLoginDto user { get; set; }
}

public class UserLoginDto
{
    public string id { get; set; }
    public string email { get; set; }
    public string username { get; set; }
    public string role { get; set; }
}

public class SessionTokenDto : Common
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string HashedPassword { get; set; }
    public string? Salt { get; set; }
    public DateTime LastLogin { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UserBookingsResponse
{
    public string Message { get; set; }
    [JsonPropertyName("bookingsDto")]
    public List<BookingRoomsDto> bookingRooms { get; set; }
}