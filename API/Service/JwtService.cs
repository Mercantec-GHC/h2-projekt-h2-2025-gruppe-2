using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainModels;

namespace API.Service;

/// <summary>
/// Service for handling JWT tokens, including generation, validation, and decoding.
/// </summary>
public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtService"/> class with configuration settings.
    /// </summary>
    /// <param name="configuration">The application configuration for JWT settings.</param>
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

    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A JWT token as a string.</returns>
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        // Add role claim if the user has a role
        if (user.Roles != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Roles.Name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes).AddHours(2),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>A <see cref="ClaimsPrincipal"/> if the token is valid; otherwise, null.</returns>
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

    /// <summary>
    /// Extracts the user ID from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>The user ID as a string, or null if not found.</returns>
    public string? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Extracts all claims from a JWT token as a dictionary.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>A dictionary of claim types and values, or null if the token is invalid.</returns>
    public Dictionary<string, string>? GetClaimsFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
    }

    /// <summary>
    /// Checks if a JWT token is expired.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>True if the token is expired; otherwise, false.</returns>
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