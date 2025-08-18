using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Service;
using DomainModels;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomsController"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data access.</param>
        public RoomsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all rooms.
        /// </summary>
        /// <returns>A list of all rooms.</returns>
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
        public async Task<ActionResult<IEnumerable<Room>>> GetUncleanRooms()
        {
            try
            {
                var rooms = await _context.Rooms.Where(r => r.Clean == false).ToListAsync();
                return Ok(new { rooms = rooms });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Der skete en fejl: " + e.Message);
            }
        }

        /// <summary>
        /// Gets a specific room by its ID.
        /// </summary>
        /// <param name="id">The ID of the room to retrieve.</param>
        /// <returns>The room with the specified ID, or 404 if not found.</returns>
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
            var timeHelper = new TimeService();
            DateTime copenhagenTime = timeHelper.GetCopenhagenTime();
            var user = new Room
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

            _context.Rooms.Add(user);
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
        /// Gets all available rooms for a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the desired availability period (UTC).</param>
        /// <param name="endDate">The end date of the desired availability period (UTC).</param>
        /// <returns>A list of available rooms, or no content if none are available.</returns>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<Room>>> GetAvailableRooms(DateTime startDate, DateTime endDate)
        {
            // 2025-08-14
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

            // Gets rooms where IDs from the unavailable list are excluded.
            List<Room> availableRooms = await _context.Rooms
                .AsNoTracking()
                .Where(room => !unavailableRoomIds.Contains(room.Id))
                .ToListAsync();

            if (availableRooms.Count == 0)
                return NoContent();

            return Ok(availableRooms);
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
    }
}