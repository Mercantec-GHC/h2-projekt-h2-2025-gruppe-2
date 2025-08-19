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
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly TimeService _timeService = new();
        private const int workFactor = 12;

        public UsersController(AppDBContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("En bruger med denne email findes allerede.");

            string salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

            // Find standard User rolle
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
                return BadRequest("Standard brugerrolle ikke fundet.");

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

            return Ok(new { message = "Bruger oprettet!", user.Email, role = userRole.Name });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            User? user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
                return Unauthorized("Forkert email eller adgangskode");

            user.LastLogin = _timeService.GetCopenhagenTime();
            await _context.SaveChangesAsync();

            // Generer JWT token josh
            string token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Login godkendt!",
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    username = user.Username,
                    role = user.Roles?.Name ?? "Loser"
                }
            });
        }

        [Authorize(Roles = "User,Admin,CleaningStaff,Reception")]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // 1. Hent ID fra token (typisk sat som 'sub' claim ved oprettelse af JWT)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized("Bruger-ID ikke fundet i token.");

            // 2. Slå brugeren op i databasen
            var user = _context.Users
                .Include(u => u.Roles) // inkluder relaterede data hvis nødvendigt
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound("Brugeren blev ikke fundet i databasen.");

            // 3. Returnér ønskede data - fx til profilsiden
            return Ok(new
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = user.Roles?.Name
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
        [HttpPatch("edit/role")]
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

            return Ok(
                $"Role for user {id} got updated from {user.Roles.ResolveRoleId(beforeRoleId)} to {user.Roles.ResolveRoleId(user.RoleId)}");
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
                return NotFound($"Bruger med id '{dto.Id}' blev ikke fundet.");

            if (!string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                    BadRequest("A user with this email already exists");
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
                return BadRequest("Internal server error :(: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Unaccounted internal server error :(: " + ex.Message);
            }

            return Ok(new
            {
                message = "Bruger opdateret!",
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
        /// Changes the password of a specified user with BCrypt.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose password will be changed.</param>
        /// <param name="newPassword">The new plain-text password to be set for the user.</param>
        /// <returns>
        /// <para>
        /// 200 OK if the user's password was successfully changed.<br/>
        /// 404 NotFound if the user with the specified ID does not exist.<br/>
        /// 400 BadRequest if an error occurs while updating the password in the database.
        /// </para>
        /// </returns>

        [Authorize(Roles = "Admin")]
        [HttpPut("edit/password")]
        public async Task<IActionResult> ChangePassword(string userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound($"User with '{userId}' not found.");
            string salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
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
}