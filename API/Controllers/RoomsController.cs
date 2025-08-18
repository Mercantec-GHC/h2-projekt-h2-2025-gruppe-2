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
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

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

        private bool RoomExists(string id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}