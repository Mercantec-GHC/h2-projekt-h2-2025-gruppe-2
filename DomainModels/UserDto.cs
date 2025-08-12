using System.ComponentModel.DataAnnotations;

namespace DomainModels;

public class RegisterDto
{
    [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Brugernavn er påkrævet")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "Adgangskode er påkrævet")]
    [MinLength(8, ErrorMessage = "Adgangskoden skal være mindst 8 tegn lang")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Adgangskoden skal indeholde mindst ét tal, ét stort bogstav, ét lille bogstav og et specialtegn")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
    [Required(ErrorMessage = "Email er påkrævet")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Adgangskode er påkrævet")]
    [MinLength(8, ErrorMessage = "Adgangskoden skal være mindst 8 tegn lang")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Adgangskoden skal indeholde mindst ét tal, ét stort bogstav, ét lille bogstav og et specialtegn")]
    public string Password { get; set; } = string.Empty;
}