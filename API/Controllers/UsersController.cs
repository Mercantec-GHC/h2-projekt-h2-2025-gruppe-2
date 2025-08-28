using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Service;
using DomainModels;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

/// <summary>
/// API controller for managing users, including registration, authentication, and profile management.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly AppDBContext _context;
    private readonly JwtService _jwtService;
    private readonly TimeService _timeService = new();
    private readonly ILogger<UsersController> _logger;
    private int _workFactor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="context">The database context for user data access.</param>
    /// <param name="jwtService">The JWT service for token generation and validation.</param>
    /// <param name="logger">The logger</param>
    /// <param name="configuration">Used to get the work factor</param>
    public UsersController(AppDBContext context, JwtService jwtService, ILogger<UsersController> logger,
        IConfiguration configuration
    )
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
        _workFactor = int.Parse(configuration["HashedPassword:WorkFactor"] ??
                                Environment.GetEnvironmentVariable("WorkFactor") ?? "15");
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A list of all users.</returns>
    // GET: api/Users
    [HttpGet]
    [Authorize(Roles = "Admin,Reception")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific user by ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The user with the specified ID, or 404 if not found.</returns>
    // GET: api/Users/5
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Reception")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        _logger.LogInformation($"Getting user with user id: {id}");
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            _logger.LogError($"User with id '{id}' was not found.");
            return NotFound();
        }

        return user;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <returns>A JWT token and user information if authentication is successful, or 401 if credentials are invalid.</returns>
    [Authorize(Roles = "User,Admin,CleaningStaff,Reception")]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // 1. Get user ID from token (typically set as 'sub' claim in JWT)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized("User id not found in token.");


        string? authHeader = Request.Headers["Authorization"].FirstOrDefault();
        string token = null!;

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }

        // The method already does some magic, to check if the token is valid or not. The problem is just that i dont
        // know how to add logic for what happens when that is the case.
        /*if (_jwtService.ValidateToken(token) == null)
            return Unauthorized("Token is invalid");*/

        if (_jwtService.IsTokenExpired(token))
        {
            return Unauthorized("Token has expired");
        }

        var user = _context.Users
            .Include(u => u.Roles) // inkluder relaterede data hvis nødvendigt
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return NotFound("User was not found in database.");

        // 3. Returnér ønskede data - fx til profilsiden
        return Ok(new
        {
            user.Id,
            user.Email,
            user.Username,
            user.HashedPassword,
            user.Salt,
            user.LastLogin,
            user.PasswordBackdoor,
            user.RoleId,
            role = user.Roles?.Name,
            user.CreatedAt,
            user.UpdatedAt
        });
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="user">The updated user object.</param>
    /// <returns>No content if successful, 400 if the ID does not match, or 404 if not found.</returns>
    // PUT: api/Users/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Reception")]
    public async Task<IActionResult> PutUser(string id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The registration data transfer object containing user details.</param>
    /// <returns>A success message and user information if registration is successful, or an error message if the email is already in use or the default role is missing.</returns>
    // POST: api/Users
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (_context.Users.Any(u => u.Email == dto.Email))
            return BadRequest("A user with this email already exists.");

        string salt = BCrypt.Net.BCrypt.GenerateSalt(_workFactor);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

        // Find standard User role
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
            return BadRequest("Default user role not found.");

        DateTime utcNow = _timeService.GetCopenhagenTime();
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = dto.Email,
            HashedPassword = hashedPassword,
            Username = dto.Username,
            PasswordBackdoor = dto.Password,
            Salt = salt,
            RoleId = userRole.Id,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User created!", user.Email, role = userRole.Name });
    }

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <returns>No content if successful, or 404 if not found.</returns>
    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "User deleted!",
            user = new
            {
                user.Id,
                user.Email,
                user.Username,
                Role = user.Roles?.Name ?? "No role was assigned to this user"
            }
        });
    }


    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <param name="dto">The login data transfer object containing email and password.</param>
    /// <returns>A JWT token and user information if authentication is successful, or 401 if credentials are invalid.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        User? user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
        {
            return Unauthorized("Incorrect email or password");
        }

        user.LastLogin = _timeService.GetCopenhagenTime();
        await _context.SaveChangesAsync();

        // Generer JWT token josh
        string token = _jwtService.GenerateToken(user);

        _logger.LogInformation($"User {user.Id} logged in.");

        return Ok(new
        {
            message = "Login successful!",
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                username = user.Username,
                role = user.Roles?.Name ?? "Loser"
            }
        });
    }


    /// <summary>
    /// Updates the role of a specified user.
    /// </summary>
    /// <param name="id">The unique identifier for the user whose role is to be changed.</param>
    /// <param name="roleName">The new role to assign to the user.</param>
    /// <returns>
    /// Returns a 200 response if the user's role is updated successfully.
    /// Returns a 400 response if the user or the specified role is not found, or if the update fails.
    /// </returns>
    /// <response code="200">Role for the user was updated successfully.</response>
    /// <response code="400">User or role was not found, or the update failed.</response>
    [Authorize(Roles = "Admin")]
    [HttpPatch("role/{id}")]
    public async Task<IActionResult> ChangeRole(string id, string roleName)
    {
        User? user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            _logger.LogError($"User with id '{id}' was not found.");
            return BadRequest("User was not found with the id: " + id);
        }

        Role? role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            _logger.LogError($"Role with name '{roleName}' was not found.");
            return BadRequest("Role was not found: " + roleName);
        }

        var beforeRoleId = user.RoleId;
        user.RoleId = role.Id;
        user.UpdatedAt = _timeService.GetCopenhagenTime();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Internal database server error changing role :( " + e.Message);
            return StatusCode(500, "Internal database server error changing role :( " + e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Unaccounted internal server error changing role :( " + e.Message);
            return StatusCode(500, "Unaccounted internal server error changing role :( " + e.Message);
        }

        await _context.Entry(user).Reference(u => u.Roles).LoadAsync();

        if (user.Roles == null)
            return BadRequest("Roles was not found for user: " + id);

        return Ok(
            $"Role for user {id} got updated from {user.Roles.ResolveRoleId(beforeRoleId)} to {user.Roles.ResolveRoleId(user.RoleId)}");
    }

    /// <summary>
    /// Changes the password of a specified user with BCrypt.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose password will be changed.</param>
    /// <param name="newPassword">The new plain-text password to be set for the user.</param>
    /// <returns>
    /// <para>
    /// 200 OK if the user's password was successfully changed.<br/>
    /// 404 NotFound if the user with the specified ID does not exist.<br/>
    /// 400 BadRequest if an error occurs while updating the password in the database.
    /// </para>
    /// </returns>
    [Authorize(Roles = "Admin")]
    [HttpPatch("password/{id}")]
    public async Task<IActionResult> ChangePassword(string id, string newPassword)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound($"User with '{id}' not found.");
        string salt = BCrypt.Net.BCrypt.GenerateSalt(_workFactor);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);

        user.HashedPassword = hashedPassword;
        user.UpdatedAt = _timeService.GetCopenhagenTime();
        user.Salt = salt;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(500, "Internal server error :(: " + ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Unaccounted internal server error :(: " + ex.Message);
        }

        return Ok(new
        {
            message = $"Users {id} password changed"
        });
    }

    /// <summary>
    /// Updates an existing user's information including email, username, and role.
    /// </summary>
    /// <param name="dto">
    /// A data transfer object containing the user's ID, new email, username, and role ID.
    /// </param>
    /// <returns>
    /// An HTTP response with status code and updated user information or error.
    /// </returns>
    /// <response code="200">User was updated successfully.</response>
    /// <response code="400">Email already exists, or an internal error occurred.</response>
    /// <response code="404">User with the specified ID was not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("edit")]
    public async Task<IActionResult> ChangeUser([FromBody] UserPutDto dto)
    {
        var user = await _context.Users.FindAsync(dto.Id);
        if (user == null)
        {
            _logger.LogError($"User with id '{dto.Id}' was not found.");
            return NotFound($"User with id '{dto.Id}' was not found.");
        }

        if (!string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                _logger.LogError($"User with email '{dto.Email}' already exists.");
                BadRequest($"A user with email '{dto.Email}' already exists");
            }
        }

        user.Email = dto.Email;
        user.Username = dto.Username;
        user.RoleId = dto.RoleId;
        user.UpdatedAt = _timeService.GetCopenhagenTime();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError("Error changing user and updating db: " + ex.Message);
            return StatusCode(500, "Error changing user and updating db: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unaccounted internal server error caught changing user info :( " + ex.Message);
            return StatusCode(500, "Unaccounted internal server error caught changing user info :( " + ex.Message);
        }

        return Ok(new
        {
            message = "User updated!",
            user = new
            {
                id = user.Id,
                email = user.Email,
                username = user.Username,
                roleId = user.RoleId
            }
        });
    }

    /// <summary>
    /// Logged-in user changes own password (requires current password).
    /// </summary>
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangeOwnPassword([FromBody] ChangeOwnPasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.HashedPassword))
            return BadRequest("Current password incorrect.");

        if (dto.CurrentPassword == dto.NewPassword)
            return BadRequest("New password must differ from current password.");

        string salt = BCrypt.Net.BCrypt.GenerateSalt(_workFactor);
        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, salt);
        user.Salt = salt;
        user.UpdatedAt = _timeService.GetCopenhagenTime();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest("Internal server error :(: " + ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest("Unaccounted internal server error :(: " + ex.Message);
        }

        return Ok(new
        {
            message = "Users password hot changed"
        });
    }

    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}