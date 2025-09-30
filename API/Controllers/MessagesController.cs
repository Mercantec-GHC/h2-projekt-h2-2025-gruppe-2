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
    /// <summary>
    /// Controls the messages for the signalR setup
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly AppDBContext _context;
        private TimeService _timeService = new();

        /// <summary>
        /// Constructs the message controller
        /// </summary>
        /// <param name="context">DB context</param>
        public MessagesController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/Messages
        /// <summary>
        /// Gets all messages
        /// </summary>
        /// <returns>A list of all the messages in as the Message class</returns>
        /// <response code="200">A list of all the messages in as the Message class</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            return await _context.Messages.ToListAsync();
        }

        // GET: api/Messages/5
        /// <summary>
        /// Gets a single message, by its ID
        /// </summary>
        /// <param name="id">ID of a message</param>
        /// <returns>A message</returns>
        /// <response code="200">A message corresponding to the given ID</response>
        /// <response code="404">The message was not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(string id)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return message;
        }

        /// <summary>
        /// Gets a users own messages, by comparing the users ID to either the sender or the destination ID. Returns
        /// an empty list of no messages are found.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>A list of the users messages</returns>
        /// <response code="200">A list of all the messages by or for one user, or an empty list</response>
        /// <response code="400">The JWT ID does not equal the users ID</response>
        /// <response code="404">User was not found</response>
        [HttpGet("by-user")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetMessagesByUserId(string userId)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userId)) return NotFound($"User '{userId}' not found.");
            
            var jwtId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (jwtId != userId) return BadRequest($"'{jwtId}' cannot get the messages of '{userId}'");

            return Ok(await _context.Messages
                .Where(m => m.UserSenderId == userId || m.UserDestinationId == userId)
                .ToListAsync());
        }

        // PUT: api/Messages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Updates an entire message
        /// </summary>
        /// <returns>No content, if everything went right</returns>
        /// <param name="id">ID of a message</param>
        /// <param name="message">The updated message</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(string id, Message message)
        {
            if (id != message.Id)
            {
                return BadRequest();
            }

            _context.Entry(message).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
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
        /// Adds message
        /// </summary>
        /// <param name="msg">The message, destination user ID and IsAdmin bool in a single DTO</param>
        /// <returns>String saying the message was added</returns>
        /// <response code="200">Confirmation string</response>
        /// <response code="400">No user ID in JWT, or user ID was not found in DB</response>
        /// <response code="409">If the message ID already exists</response>
        /// <response code="500">Unaccounted server side error</response>
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> PostMessage([FromBody] PostMessageRequestDto msg)
        {
            string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return BadRequest("User ID not found in token");

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return BadRequest("User ID not found in database");

            string id = Guid.NewGuid().ToString();
            _context.Messages.Add(new Message
            {
                Id = id,
                UserSenderId = userId,
                UserDestinationId = msg.DestinationId,
                Msg = msg.Msg,
                IsAdmin = msg.IsAdmin,
                CreatedAt = _timeService.GetCopenhagenTime(),
                UpdatedAt = _timeService.GetCopenhagenTime()
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (MessageExists(id))
                {
                    return Conflict();
                }
                else
                {
                    return StatusCode(500, "Unaccounted DB Update exception caught adding message: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Generel exception caught adding message: " + ex.Message);
            }

            return Ok("Message added to database.");
        }

        // DELETE: api/Messages/5
        /// <summary>
        /// Deletes a message by its ID
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>A response saying everything was cool</returns>
        /// <response code="204">Everything went right</response>
        /// <response code="404">Message does not exist</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(string id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MessageExists(string id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}