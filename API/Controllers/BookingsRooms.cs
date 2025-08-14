using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using DomainModels;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsRoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public BookingsRoomsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/BookingsRooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingsRooms>>> GetBookingsRooms()
        {
            return await _context.BookingsRooms.ToListAsync();
        }

        // GET: api/BookingsRooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingsRooms>> GetBookingsRooms(string id)
        {
            var bookingsRooms = await _context.BookingsRooms.FindAsync(id);

            if (bookingsRooms == null)
            {
                return NotFound();
            }

            return bookingsRooms;
        }

        // PUT: api/BookingsRooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBookingsRooms(string id, BookingsRooms bookingsRooms)
        {
            if (id != bookingsRooms.Id)
            {
                return BadRequest();
            }

            _context.Entry(bookingsRooms).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingsRoomsExists(id))
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

        // POST: api/BookingsRooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BookingsRooms>> PostBookingsRooms(BookingsRooms bookingsRooms)
        {
            _context.BookingsRooms.Add(bookingsRooms);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BookingsRoomsExists(bookingsRooms.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetBookingsRooms", new { id = bookingsRooms.Id }, bookingsRooms);
        }

        // DELETE: api/BookingsRooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookingsRooms(string id)
        {
            var bookingsRooms = await _context.BookingsRooms.FindAsync(id);
            if (bookingsRooms == null)
            {
                return NotFound();
            }

            _context.BookingsRooms.Remove(bookingsRooms);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingsRoomsExists(string id)
        {
            return _context.BookingsRooms.Any(e => e.Id == id);
        }
    }
}
