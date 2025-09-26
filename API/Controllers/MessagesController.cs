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
    public class MessagesController : ControllerBase
    {
        private readonly AppDBContext _context;
        private TimeService _timeService = new();

        public MessagesController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages()
        {
            return await _context.Messages.ToListAsync();
        }

        // GET: api/Messages/5
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
        /// Gets a users own messages.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns></returns>
        [HttpGet("by-user")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetMessagesByUserId(string userId)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == userId)) return NotFound($"User '{userId}' not found.");
            
            var jwtId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (jwtId != userId) return BadRequest($"'{jwtId}' cannot get the messages of '{userId}'");

            return Ok(await _context.Messages
                .Where(m => m.UserId == userId)
                .Select(m => m.Content)
                .ToListAsync());
        }

        // PUT: api/Messages/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        /// Add message
        /// </summary>
        /// <param name="msg">The message to be added</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> PostMessage([FromBody] string msg)
        {
            string? userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return BadRequest("User ID not found in token");

            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return BadRequest("User ID not found in database");

            string id = Guid.NewGuid().ToString();
            _context.Messages.Add(new Message
            {
                Id = id,
                UserId = userId,
                Content = msg,
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