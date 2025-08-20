using API.Data;
using API.Service;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class DataSeederController : ControllerBase
{
    private readonly AppDBContext _context;
    private readonly DataSeederService _dataSeederService = new();
    private readonly ILogger<DataSeederController> _logger;
    private readonly TimeService _timeService = new TimeService();

    public DataSeederController(AppDBContext context, ILogger<DataSeederController> logger)
    {
        _context = context;
        _logger = logger;
    }


    [HttpPost("rooms")]
    public async Task<ActionResult<IEnumerable<Room>>> SeedRooms(int count)
    {
        List<Room> rooms = _dataSeederService.SeedRooms(count);
        try
        {
            _context.Rooms.AddRange(rooms);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("Updating DB error caught seeding rooms: " + ex.Message);
            Console.WriteLine("Updating DB error caught seeding rooms: " + ex.Message);
            return StatusCode(500, "Updating DB error caught seeding rooms: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Generel error caught seeding rooms: " + ex.Message);
            Console.WriteLine("Generel error caught seeding rooms: " + ex.Message);
            return StatusCode(500, "Generel error caught seeding rooms: " + ex.Message);
        }
        
        _logger.LogInformation($"{count} seeded rooms added to database: \n" + rooms);

        return Ok(new
        {
            message = $"{count} seeded rooms added to database.",
            Rooms = rooms
        });
    }
}