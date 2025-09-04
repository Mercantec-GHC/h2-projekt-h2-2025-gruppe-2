using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Service;
using DomainModels;
using Microsoft.AspNetCore.Authorization;
using DomainModels.Mapping;

namespace API.Controllers
{
    /// <summary>
    /// API controller for managing hotel bookings.
    /// Provides endpoints for creating, reading, updating, and deleting bookings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly TimeService _timeHelper = new();
        private readonly ILogger<BookingsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingsController"/> class.
        /// </summary>
        /// <param name="context">The database context to use for data access.</param>
        /// <param name="logger">Logger context for logging to a file</param>
        public BookingsController(AppDBContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all bookings.
        /// </summary>
        /// <returns>A list of all bookings.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings.ToListAsync();
        }

        /// <summary>
        /// Gets all the bookings of a user and the rooms associated with the bookings
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>A list of a users bookings and its associated rooms and/or a response code</returns>
        /// <response code="200">List of bookings and rooms for a user</response>
        [HttpGet("user/{id}")]
        // [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<BookingRoomsDto>> GetBookingsAndRoomsByUserIdAsync(string id)
        {
            List<Booking> bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
                .Where(b => b.UserId == id)
                .ToListAsync();

            var bookingsDto = BookingMappings.ToBookingRoomsDto(bookings);
            return Ok(new
            {
                message = $"Bookings and rooms for user {id} retrieved",
                bookingsDto
            });
        }

        /// <summary>
        /// Gets a specific booking by its ID.
        /// </summary>
        /// <param name="id">The ID of the booking to retrieve.</param>
        /// <returns>The booking with the specified ID, or 404 if not found.</returns>
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

        /// <summary>
        /// Updates an existing booking.
        /// </summary>
        /// <param name="id">The ID of the booking to update.</param>
        /// <param name="booking">The updated booking object.</param>
        /// <returns>No content if successful, 400 if the ID does not match, or 404 if not found.</returns>
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

        /// <summary>
        /// Creates a new booking and links it to the rooms.
        /// </summary>
        /// <param name="dto">The booking data transfer object containing booking details and room ID.</param>
        /// <returns>A success message if the booking is created.</returns>
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking([FromBody] BookingPostDto dto)
        {
            if (dto.OccupiedFrom > dto.OccupiedTill)
            {
                return BadRequest("Occupied from date must be earlier than occupied till date.");
            }
            
            DateTime copenhagenTime = _timeHelper.GetCopenhagenTime();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            string newId = Guid.NewGuid().ToString();
            var bookingId = _context.Bookings.Any(b => b.Id == newId);
            if (bookingId) return StatusCode(500, $"Booking ID '{newId}' already exists. !!!ONE IN A TRILLION!!!'");

            Booking booking = new Booking
            {
                Id = newId,
                Adults = dto.Adults,
                Children = dto.Children,
                RoomService = dto.RoomService,
                Dinner = dto.Dinner,
                TotalPrice = dto.TotalPrice,
                UserId = userId,
                OccupiedFrom = dto.OccupiedFrom,
                OccupiedTill = dto.OccupiedTill,
                CreatedAt = copenhagenTime,
                UpdatedAt = copenhagenTime
            };

            try
            {
                _context.Bookings.Add(booking);

                foreach (string roomId in dto.RoomIds)
                {
                    var tempRoom = _context.Rooms.Any(r => r.Id == roomId);
                    if (!tempRoom)
                    {
                        return StatusCode(500, $"Room '{roomId}' not found'");
                    }

                    BookingsRooms bookingsRooms = new BookingsRooms
                    {
                        Id = Guid.NewGuid().ToString(),
                        BookingId = booking.Id,
                        RoomId = roomId,
                        CreatedAt = copenhagenTime,
                        UpdatedAt = copenhagenTime
                    };
                    _context.BookingsRooms.Add(bookingsRooms);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError("Error updating db with new bookings: " + e.Message);
                return StatusCode(500, "Error updating db with new bookings: " + e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Generel error saving new bookings: " + e.Message);
                return StatusCode(500, "Generel error saving new bookings: " + e.Message);
            }

            _logger.LogInformation("Bookings added: " + booking);
            return Ok(new { message = "Booking created successfully!" });
        }

        /// <summary>
        /// Deletes a booking by its ID.
        /// </summary>
        /// <param name="id">The ID of the booking to delete.</param>
        /// <returns>No content if successful, or 404 if not found.</returns>
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

        /// <summary>
        /// Checks if a booking exists in the database.
        /// </summary>
        /// <param name="id">The ID of the booking to check.</param>
        /// <returns>True if the booking exists, otherwise false.</returns>
        private bool BookingExists(string id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}