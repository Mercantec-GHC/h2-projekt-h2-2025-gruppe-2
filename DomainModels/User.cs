using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

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
    [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Brugernavn er påkrævet")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adgangskode er påkrævet")]
    [MinLength(8, ErrorMessage = "Adgangskoden skal være mindst 8 tegn lang")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage =
            "Adgangskoden skal indeholde mindst ét tal, ét stort bogstav, ét lille bogstav og et specialtegn")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adgangskode er påkrævet")]
    [MinLength(8, ErrorMessage = "Adgangskoden skal være mindst 8 tegn lang")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage =
            "Adgangskoden skal indeholde mindst ét tal, ét stort bogstav, ét lille bogstav og et specialtegn")]
    public string Password { get; set; } = string.Empty;
}

public class UserGetDto
{
    public string Id { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

public class UserPutDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
}