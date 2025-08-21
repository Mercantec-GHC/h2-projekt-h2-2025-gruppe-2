using API.Data;
using API.Service;
using DomainModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

/// <summary>
/// Bogus data creator
/// </summary>
public class DataSeederController : ControllerBase
{
    private readonly AppDBContext _context;
    private readonly DataSeederService _dataSeederService;
    private readonly ILogger<DataSeederController> _logger;

    /// <summary>
    /// Constructs the DataSeederController
    /// </summary>
    /// <param name="context">DB context</param>
    /// <param name="logger">Logger context</param>
    /// <param name="dataSeederService">Logic for creating seeds</param>
    public DataSeederController(AppDBContext context, ILogger<DataSeederController> logger, DataSeederService dataSeederService)
    {
        _context = context;
        _logger = logger;
        _dataSeederService = dataSeederService;
    }


    /// <summary>
    /// Creates an amount of rooms.
    /// </summary>
    /// <param name="count">Amount of objects to be created</param>
    /// <returns>A status code and the rooms that got created</returns>
    /// <response code="200">List of rooms that got created and/or an http code</response>
    /// <response code="500">Returns an error message indicating what the server problem was</response>
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
    
    /// <summary>
    /// Creates an amount of users
    /// </summary>
    /// <param name="count">Amount of objects to be created</param>
    /// <returns>List of users created and/or a http code</returns>
    /// <response code="200">List of rooms that got created</response>
    /// <response code="500">Returns an error message indicating what the server problem was</response>
    [HttpPost("users")]
    public async Task<ActionResult<IEnumerable<Room>>> SeedUsers(int count)
    {
        List<User> users = _dataSeederService.SeedUsers(count);
        try
        {
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError("Updating DB error caught seeding users: " + ex.Message);
            Console.WriteLine("Updating DB error caught seeding users: " + ex.Message);
            return StatusCode(500, "Updating DB error caught seeding users: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Generel error caught seeding users: " + ex.Message);
            Console.WriteLine("Generel error caught seeding users: " + ex.Message);
            return StatusCode(500, "Generel error caught seeding users: " + ex.Message);
        }
        
        _logger.LogInformation($"{count} seeded users added to database: \n" + users);

        return Ok(new
        {
            message = $"{count} seeded users added to database.",
            Users = users
        });
    }
}