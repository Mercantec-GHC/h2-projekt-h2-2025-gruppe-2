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
    /// <summary>
    /// API controller for managing the relationship between bookings and rooms.
    /// Provides endpoints for creating, reading, updating, and deleting BookingsRooms entities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsRoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingsRoomsController"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data access.</param>
        public BookingsRoomsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all BookingsRooms entries.
        /// </summary>
        /// <returns>A list of all BookingsRooms entities.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingsRooms>>> GetBookingsRooms()
        {
            return await _context.BookingsRooms.ToListAsync();
        }

        /// <summary>
        /// Gets a specific BookingsRooms entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the BookingsRooms entry to retrieve.</param>
        /// <returns>The BookingsRooms entry with the specified ID, or 404 if not found.</returns>
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

        /// <summary>
        /// Updates an existing BookingsRooms entry.
        /// </summary>
        /// <param name="id">The ID of the BookingsRooms entry to update.</param>
        /// <param name="bookingsRooms">The updated BookingsRooms object.</param>
        /// <returns>No content if successful, 400 if the ID does not match, or 404 if not found.</returns>
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

        /// <summary>
        /// Creates a new BookingsRooms entry.
        /// </summary>
        /// <param name="bookingsRooms">The BookingsRooms object to create.</param>
        /// <returns>The created BookingsRooms entry, or a conflict if the ID already exists.</returns>
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

        /// <summary>
        /// Deletes a BookingsRooms entry by its ID.
        /// </summary>
        /// <param name="id">The ID of the BookingsRooms entry to delete.</param>
        /// <returns>No content if successful, or 404 if not found.</returns>
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

        /// <summary>
        /// Checks if a BookingsRooms entry exists in the database.
        /// </summary>
        /// <param name="id">The ID of the BookingsRooms entry to check.</param>
        /// <returns>True if the entry exists, otherwise false.</returns>
        private bool BookingsRoomsExists(string id)
        {
            return _context.BookingsRooms.Any(e => e.Id == id);
        }
    }
}
