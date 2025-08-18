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
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private TimeService _timeService = new TimeService();

        public RoomsController(AppDBContext context)
        {
            _context = context;
        }
        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        // GET: api/Rooms/unclean
        [HttpGet("unclean")]
        [Authorize(Roles = "CleaningStaff,Admin,Reception")]
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
                return BadRequest("An error has occured: " + e.Message);
            }
        }


        // GET: api/Rooms/clean
        [HttpGet("clean")]
        [Authorize(Roles = "CleaningStaff,Admin,Reception")]
        public async Task<ActionResult<IEnumerable<Room>>> GetCleanRooms()
        {
            try
            {
                var rooms = await _context.Rooms.Where(r => r.Clean == true).ToListAsync();
                return Ok(new { rooms = rooms });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("An error has occured: " + e.Message);
            }
        }

        // PATCH: api/Rooms/{id}/clean
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

        // GET: api/Rooms/5
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

        // PUT: api/Rooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // POST: api/Rooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // DELETE: api/Rooms/5
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
        /// Retrieves all rooms based on availability, within the specified date range (YYYY-MM-DDTHH:MM:SS/2025-08-18T10:30:00).
        /// </summary>
        /// <param name="startDate">The start date of the requested availability period</param>
        /// <param name="endDate">The end date of the requested availability period</param>
        /// <param name="available">Get the rooms based on if they are available or unavailable</param>
        /// <returns>A response code and a list of available/unavailable rooms</returns>
        /// <response code="200">List of available/unavailable rooms found</response>
        /// <response code="400">End date can't be lower than the starting date</response>
        [HttpGet("availability")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByAvailability(DateTime startDate, DateTime endDate, bool available = true)
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
                return Ok(available ? "All rooms are unavailable in the specified date range." : "All rooms are available in the specified date range.");

            return Ok(availableRooms);
        }

        private bool RoomExists(string id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}