using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Service;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Controllers
{
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
        private const int workFactor = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="context">The database context for user data access.</param>
        /// <param name="jwtService">The JWT service for token generation and validation.</param>
        public UsersController(AppDBContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
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
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Reception")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="user">The updated user object.</param>
        /// <returns>No content if successful, 400 if the ID does not match, or 404 if not found.</returns>
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
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("A user with this email already exists.");
            
            string salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
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
                return Unauthorized("Incorrect email or password");
            
            user.LastLogin = DateTime.UtcNow.AddHours(2);
            await _context.SaveChangesAsync();

            // Generate JWT token
            string token = _jwtService.GenerateToken(user);

            return Ok(new {
                message = "Login successful!", 
                token = token,
                user = new {
                    id = user.Id,
                    email = user.Email,
                    username = user.Username,
                    role = user.Roles?.Name ?? "Loser"
                }
            });
        }
        
        /// <summary>
        /// Retrieves the currently authenticated user's profile information.
        /// </summary>
        /// <returns>The current user's profile data, or 401/404 if not found or unauthorized.</returns>
        [Authorize(Roles = "User,Admin,CleaningStaff,Reception")]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // 1. Get user ID from token (typically set as 'sub' claim in JWT)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized("User ID not found in token.");

            // 2. Look up the user in the database
            var user = _context.Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found in the database.");

            // 3. Return desired data, e.g., for profile page
            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = user.Roles?.Name
            });
        }

        /// <summary>
        /// Checks if a user exists in the database.
        /// </summary>
        /// <param name="id">The ID of the user to check.</param>
        /// <returns>True if the user exists, otherwise false.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("change/role")]
        public async Task<IActionResult> ChangeRole(string id, string roleName)
        {
            User? user = await _context.Users.FindAsync(id);
            if (user == null)
                return BadRequest("User was not found with the id: " + id);
            
            
            Role? role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return BadRequest("Role was not found: " + roleName);
            
            var beforeRoleId = user.RoleId;
            user.RoleId = role.Id;
            user.UpdatedAt = _timeService.GetCopenhagenTime();
            var updatedUser = await _context.SaveChangesAsync();
            if (updatedUser == 0)
                return BadRequest("User was not updated");
            
            await _context.Entry(user).Reference(u => u.Roles).LoadAsync();
            
            if (user.Roles == null)
                return BadRequest("Roles was not found for user: " + id);
            
            return Ok($"Role for user {id} got updated from {user.Roles.ResolveRoleId(beforeRoleId)} to {user.Roles.ResolveRoleId(user.RoleId)}");
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
