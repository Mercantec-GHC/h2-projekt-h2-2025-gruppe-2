using System.Security.Cryptography;
using System.Text;

namespace DomainModels;

public class User : Common
{
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string HashedPassword { get; set; }
    public required string Salt {  get; set; }
    public DateTime LastLogin { get; set; }
    
    public string PasswordBackdoor {  get; set; } 
    // Only for educational purposes, not in the final product!
    
    public static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder();
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

}