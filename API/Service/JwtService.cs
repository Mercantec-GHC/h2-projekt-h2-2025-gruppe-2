using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainModels;

namespace API.Service;

/// Service til håndtering af JWT tokens - generering, validering og decoding
public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"]
                     ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                     ?? "NOTSAFENOTSAFENOTSAFENOTSAFENOTSAFENOTSAFENOTSAFENOTSAFE";

        _issuer = _configuration["Jwt:Issuer"]
                  ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
                  ?? "H2-2025-API";

        _audience = _configuration["Jwt:Audience"]
                    ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                    ?? "H2-2025-Client";

        _expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"]
                                   ?? Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES")
                                   ?? "60");
    }

    /// Genererer en JWT token for en bruger
    /// <param name="user">Brugeren der skal have en token</param>
    /// <returns>JWT token som string</returns>
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("userId", user.Id),
            new Claim("username", user.Username),
            new Claim("email", user.Email)
        };

        // Tilføj rolle claim hvis brugeren har en rolle
        if (user.Roles != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Roles.Name));
            claims.Add(new Claim("role", user.Roles.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// Validerer en JWT token
    /// <param name="token">Token der skal valideres</param>
    /// <returns>ClaimsPrincipal hvis token er gyldig, ellers null</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token,
                validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// Udtrækker bruger ID fra en JWT token
    /// <param name="token">JWT token</param>
    /// <returns>Bruger ID som string, eller null hvis ikke fundet</returns>
    public string? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// Udtrækker claims fra en JWT token
    /// <param name="token">JWT token</param>
    /// <returns>Dictionary med claims, eller null</returns>
    public Dictionary<string, string>? GetClaimsFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
    }

    /// Tjekker om en token er udløbet
    /// <param name="token">JWT token</param>
    /// <returns>True hvis token er udløbet, ellers false</returns>
    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}