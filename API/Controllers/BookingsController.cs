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
    public class BookingsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public BookingsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/Bookings
        /// <summary>
        /// Gets all bookings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings.ToListAsync();
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(string id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        // PUT: api/Bookings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(string id, Booking booking)
        {
            if (id != booking.Id)
            {
                return BadRequest();
            }

            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
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

        // POST: api/Bookings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Creates a new booking
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking([FromBody] BookingPostDto dto)
        {
            TimeService timeHelper = new();
            DateTime copenhagenTime = timeHelper.GetCopenhagenTime();
            Booking booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                Adults = dto.Adults,
                Children = dto.Children,
                RoomService = dto.RoomService,
                Dinner = dto.Dinner,
                UserId = dto.UserId,
                OccupiedFrom = dto.OccupiedFrom,
                OccupiedTill = dto.OccupiedTill,
                CreatedAt = copenhagenTime,
                UpdatedAt = copenhagenTime
            };
            
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            BookingsRooms bookingsRooms = new BookingsRooms
            {
                Id = Guid.NewGuid().ToString(),
                BookingId = booking.Id,
                RoomId = dto.RoomId,
                CreatedAt = copenhagenTime,
                UpdatedAt = copenhagenTime
            };
                
            _context.BookingsRooms.Add(bookingsRooms);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Booking er oprettet!" });
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(string id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        private bool BookingExists(string id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
