using Bogus;
using DomainModels;

namespace API.Service;

/// <summary>
/// Adds Bogus data to the DB
/// </summary>
public class DataSeederService
{
    private readonly ILogger<DataSeederService> _logger;
    private readonly TimeService _timeService = new TimeService();
    private int _workFactor;

    /// <summary>
    /// Constructor for the class
    /// </summary>
    /// <param name="logger">Logger configuration</param>
    /// <param name="configuration">Configuration</param>
    public DataSeederService(ILogger<DataSeederService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _workFactor = int.Parse(configuration["HashedPassword:WorkFactor"] ??
                                Environment.GetEnvironmentVariable("WorkFactor") ?? "15");
    }

    /// <summary>
    /// Creates Bogus room data
    /// </summary>
    /// <param name="count">Amount of objects to be created</param>
    /// <returns>A list of newly created rooms</returns>
    public List<Room> SeedRooms(int count)
    {
        var faker = new Faker<Room>("en")
            .RuleFor(r => r.Id, Guid.NewGuid().ToString())
            .RuleFor(r => r.KingBeds, f => f.Random.Int(0, 3))
            .RuleFor(r => r.QueenBeds, f => f.Random.Int(0, 3))
            .RuleFor(r => r.TwinBeds, f => f.Random.Int(0, 3))
            .RuleFor(r => r.Size, f => f.Random.Int(10, 30))
            .RuleFor(r => r.Tv, f => f.Random.Int(0, 2))
            .RuleFor(r => r.Bathroom, f => f.Random.Bool())
            .RuleFor(r => r.Clean, f => f.Random.Bool())
            .RuleFor(r => r.Bathtub, f => f.Random.Bool())
            .RuleFor(r => r.WiFi, f => f.Random.Bool())
            .RuleFor(r => r.Fridge, f => f.Random.Bool())
            .RuleFor(r => r.Stove, f => f.Random.Bool())
            .RuleFor(r => r.Oven, f => f.Random.Bool())
            .RuleFor(r => r.Description, f => f.Lorem.Paragraphs(1))
            .RuleFor(r => r.Microwave, f => f.Random.Bool())
            .RuleFor(r => r.Price, f => f.Random.Decimal(200, 2000))
            .RuleFor(r => r.CreatedAt, _timeService.GetCopenhagenTime())
            .RuleFor(r => r.UpdatedAt, (f, r) =>
                f.Date.Between(r.CreatedAt, DateTime.UtcNow.AddHours(2)));

        List<Room> rooms = faker.Generate(count);
        rooms = AddBeds(rooms);

        return rooms;
    }

    private List<Room> AddBeds(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            room.Beds = room.KingBeds + room.QueenBeds + room.TwinBeds;
        }

        return rooms;
    }

    /// <summary>
    /// Creates Bogus data for the class User
    /// </summary>
    /// <param name="count">The amount of User class to be created</param>
    /// <returns>A list of Users</returns>
    public List<User> SeedUsers(int count)
    {
        var faker = new Faker<User>("en")
            .RuleFor(u => u.Id, Guid.NewGuid().ToString())
            .RuleFor(u => u.Username, f => f.Name.FirstName() + " " + f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordBackdoor, f => f.Internet.Password(
                length: 12,
                memorable: false,
                prefix: "1Aa!"))
            .RuleFor(u => u.RoleId, "1")
            .RuleFor(u => u.CreatedAt, _timeService.GetCopenhagenTime())
            .RuleFor(u => u.UpdatedAt, _timeService.GetCopenhagenTime());

        List<User> users = faker.Generate(count);
        users = HashPasswords(users);

        return users;
    }

    private List<User> HashPasswords(List<User> users)
    {
        foreach (User user in users)
        {
            user.Salt = BCrypt.Net.BCrypt.GenerateSalt(_workFactor);
            user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(user.PasswordBackdoor, user.Salt);
        }

        return users;
    }
}