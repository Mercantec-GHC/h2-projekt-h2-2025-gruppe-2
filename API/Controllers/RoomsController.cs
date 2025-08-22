using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Service;
using DomainModels;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers

{
    /// <summary>
    /// API controller for managing hotel rooms.
    /// Provides endpoints for creating, reading, updating, and deleting rooms, as well as querying room availability and cleanliness.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RoomsController> _logger;
        private TimeService _timeService = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomsController"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data access.</param>
        /// <param name="logger">Logger configuration</param>
        public RoomsController(AppDBContext context, ILogger<RoomsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all rooms.
        /// </summary>
        /// <returns>A list of all rooms.</returns>
        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        /// <summary>
        /// Gets all rooms that are currently marked as unclean.
        /// </summary>
        /// <returns>A list of unclean rooms, or a bad request if an error occurs.</returns>
        [HttpGet("unclean")]
        [Authorize(Roles = "CleaningStaff,Admin,Reception")]
        public async Task<ActionResult<IEnumerable<Room>>> GetUncleanRooms()
        {
            try
            {
                var rooms = await _context.Rooms.Where(r => r.Clean == false).ToListAsync();
                return Ok(new { rooms });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("An error has occured: " + e.Message);
            }
        }

        /// <summary>
        /// Gets a specific room by its ID.
        /// </summary>
        /// <returns>The room with the specified ID, or 404 if not found.</returns>
        // GET: api/Rooms/clean
        [HttpGet("clean")]
        [Authorize(Roles = "CleaningStaff,Admin,Reception")]
        public async Task<ActionResult<IEnumerable<Room>>> GetCleanRooms()
        {
            try
            {
                var rooms = await _context.Rooms.Where(r => r.Clean == true).ToListAsync();
                return Ok(new { rooms });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("An error has occured: " + e.Message);
            }
        }
        
        // GET: api/Rooms/5
        /// <summary>
        /// Gets a single room with the given ID
        /// </summary>
        /// <param name="id">The rooms ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }
        
        /// <summary>
        /// Retrieves all rooms based on availability, within the specified date range (YYYY-MM-DDTHH:MM:SS/2025-08-18T10:30:00).
        /// </summary>
        /// <param name="startDate">The start date of the requested availability period</param>
        /// <param name="endDate">The end date of the requested availability period</param>
        /// <param name="available">Get the rooms based on if they are available or unavailable</param>
        /// <returns>A response code and a list of available/unavailable rooms</returns>
        /// <response code="200">List of available/unavailable rooms found</response>
        /// <response code="400">End date can't be lower than the starting date</response>
        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByAvailability(DateTime startDate, DateTime endDate,
            bool available = true)
        {
            // 2025-08-18T10:30:00
            // Formets to UTC, for PostgreSQL compatability.
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            if (startDate >= endDate)
                return BadRequest("startDate must be earlier than endDate.");

            // Gets the IDs of rooms that are occupied from the date range.
            List<string> unavailableRoomIds = await _context.BookingsRooms
                .Where(br =>
                    br.Booking.OccupiedFrom < endDate &&
                    br.Booking.OccupiedTill > startDate)
                .Select(br => br.RoomId)
                .ToListAsync();

            // Gets rooms where IDs from the unavailable list are included or excluded.
            List<Room> availableRooms = await _context.Rooms
                .AsNoTracking() // Since we are only reading the data, we don't track it.
                .Where(room => available ? !unavailableRoomIds.Contains(room.Id) : unavailableRoomIds.Contains(room.Id))
                .ToListAsync();

            if (availableRooms.Count == 0)
                return Ok(available
                    ? "All rooms are unavailable in the specified date range."
                    : "All rooms are available in the specified date range.");

            return Ok(availableRooms);
        }

        // PATCH: api/Rooms/{id}/clean
        /// <summary>
        /// Changes a room to be clean with the given ID
        /// </summary>
        /// <param name="id">The rooms ID</param>
        /// <returns></returns>
        [HttpPatch("{id}/clean")]
        [Authorize(Roles = "CleaningStaff,Admin")]
        public async Task<IActionResult> CleanRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            if (room.Clean) return BadRequest("Error. Room already marked clean.");

            room.Clean = true;
            room.UpdatedAt = _timeService.GetCopenhagenTime();
            await _context.SaveChangesAsync();

            return Ok(new { room.Id, room.Clean });
        }

        // PATCH: api/Rooms/{id}/unclean
        /// <summary>
        /// Changes a room to be unclean with the given ID
        /// </summary>
        /// <param name="id">The rooms ID</param>
        /// <returns></returns>
        [HttpPatch("{id}/unclean")]
        [Authorize(Roles = "CleaningStaff,Admin")]
        public async Task<IActionResult> UncleanRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            if (!room.Clean) return BadRequest("Error. Room already marked unclean.");

            room.Clean = false;
            room.UpdatedAt = _timeService.GetCopenhagenTime();
            await _context.SaveChangesAsync();

            return Ok(new { room.Id, room.Clean });
        }

        /// <summary>
        /// Updates an existing room.
        /// </summary>
        /// <param name="id">The ID of the room to update.</param>
        /// <param name="room">The updated room object.</param>
        /// <returns>No content if successful, 400 if the ID does not match, or 404 if not found.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(string id, Room room)
        {
            if (id != room.Id)
            {
                return BadRequest();
            }

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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
        /// Creates a new room.
        /// </summary>
        /// <param name="dto">The room data transfer object containing room details.</param>
        /// <returns>A success message, the new room's ID, and the room data.</returns>
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom([FromBody] RoomPostDto dto)
        {
            string roomId = Guid.NewGuid().ToString();
            DateTime copenhagenTime = _timeService.GetCopenhagenTime();
            var room = new Room
            {
                Id = roomId,
                WiFi = dto.WiFi,
                Bathroom = dto.Bathroom,
                Bathtub = dto.Bathtub,
                Beds = dto.Beds,
                Clean = dto.Clean,
                KingBeds = dto.KingBeds,
                QueenBeds = dto.QueenBeds,
                Size = dto.Size,
                Tv = dto.Tv,
                TwinBeds = dto.TwinBeds,
                Fridge = dto.Fridge,
                Description = dto.Description,
                Microwave = dto.Microwave,
                Oven = dto.Oven,
                Price = dto.Price,
                Stove = dto.Stove,
                CreatedAt = copenhagenTime,
                UpdatedAt = copenhagenTime
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room er oprettet!", id = roomId, room = dto });
        }

        /// <summary>
        /// Deletes a room by its ID.
        /// </summary>
        /// <param name="id">The ID of the room to delete.</param>
        /// <returns>No content if successful, or 404 if not found.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes rooms by the given date.
        /// </summary>
        /// <param name="startDate">Starting date</param>
        /// <param name="endDate">Ending date</param>
        /// <returns>No content if successful, or 404 if not found.</returns>
        [HttpDelete("date")]
        public async Task<IActionResult> DeleteRoomsByDate(DateTime startDate, DateTime endDate)
        {
            DateTime utcStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            DateTime utcEndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            List<Room> rooms = await _context.Rooms.Where(r =>
                    r.CreatedAt >= utcStartDate &&
                    r.CreatedAt <= utcEndDate)
                .ToListAsync();
            if (!rooms.Any())
            {
                return NotFound(new { message = "No rooms found with the given dates.", startDate, endDate });
            }

            try
            {
                _context.Rooms.RemoveRange(rooms);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError("Updating db error caught deleting rooms by date: " + ex.Message);
                Console.WriteLine("Updating db error caught deleting rooms by date: " + ex.Message);
                return StatusCode(500, "Updating db error caught deleting rooms by date: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Generel error caught deleting rooms by date: " + ex.Message);
                Console.WriteLine("Generel error caught deleting rooms by date: " + ex.Message);
                return StatusCode(500, "Generel error caught deleting rooms by date: " + ex.Message);
            }

            return Ok(new { message = "Rooms got deleted!", rooms });
        }

        /// <summary>
        /// Checks if a room exists in the database.
        /// </summary>
        /// <param name="id">The ID of the room to check.</param>
        /// <returns>True if the room exists, otherwise false.</returns>
        private bool RoomExists(string id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        /// <summary>
        /// Returns rooms that a certain user has booked
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>An internet code and a list of booked rooms</returns>
        [HttpGet("userId")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByUserId(string userId)
        {
            try
            {
                List<Room> rooms = await _context.Rooms
                    .Where(r => _context.BookingsRooms
                        .Any(br => br.RoomId == r.Id &&
                                   _context.Bookings.Any(b => b.Id == br.BookingId && b.UserId == userId)))
                    .ToListAsync();
                
                return Ok(new
                {
                    message = rooms.Count == 0
                        ? $"User {userId} has not booked any rooms"
                        : $"User {userId} has booked the following rooms",
                    rooms
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Generel error caught getting rooms by user id: " + ex.Message);
                return StatusCode(500, "Generel error caught getting rooms by user id: " + ex.Message);
            }
        }
    }
}