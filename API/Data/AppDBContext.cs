using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

/// <summary>
/// Entity Framework Core database context for the application.
/// Manages access to users, roles, bookings, rooms, and booking-room relationships.
/// </summary>
public class AppDBContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDBContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the users in the system.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the roles available in the system.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the bookings made by users.
    /// </summary>
    public DbSet<Booking> Bookings { get; set; } = null!;

    /// <summary>
    /// Gets or sets the rooms available in the hotel.
    /// </summary>
    public DbSet<Room> Rooms { get; set; } = null!;

    /// <summary>
    /// Gets or sets the relationships between bookings and rooms.
    /// </summary>
    public DbSet<BookingsRooms> BookingsRooms { get; set; } = null!;

    /// <summary>
    /// History for users, when they write a message in SignalR
    /// </summary>
    public DbSet<Message> Messages { get; set; }

    /// <summary>
    /// Configures the entity relationships and seeds initial data for roles and rooms.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            // Name must be unique
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Email must be unique
            entity.HasIndex(u => u.Email).IsUnique();

            // Configure foreign key to Role
            entity.HasOne(u => u.Roles)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-one relation with bookings
            entity.HasMany(u => u.Bookings)
                .WithOne(b => b.User);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            /*entity.HasOne(b => b.User)
                .WithMany(u => u.Bookings);*/
        });

        modelBuilder.Entity<Room>(entity => { });

        modelBuilder.Entity<BookingsRooms>()
            .HasOne(br => br.Booking)
            .WithMany(br => br.BookingRooms)
            .HasForeignKey(br => br.BookingId);

        modelBuilder.Entity<BookingsRooms>()
            .HasOne(br => br.Room)
            .WithMany(br => br.BookingRooms)
            .HasForeignKey(br => br.RoomId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.User) // Navigation property on Message
            .WithMany() // If you want to ignore a collection on User, use parameterless WithMany()
            .HasForeignKey(m => m.UserSenderId);

        // Seed roles and test rooms (for development only)
        SeedRoles(modelBuilder);
    }

    /// <summary>
    /// Seeds initial roles and rooms into the database for development purposes.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    private void SeedRoles(ModelBuilder modelBuilder)
    {
        var roles = new[]
        {
            new Role
            {
                Id = "1",
                Name = "User",
                Description = "Standard user with basic permissions",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "2",
                Name = "CleaningStaff",
                Description = "Cleaning staff with access to cleaning modules",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "3",
                Name = "Reception",
                Description = "Reception staff with access to bookings and guests",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "4",
                Name = "Admin",
                Description = "Administrator with full system access",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };
        var rooms = new[]
        {
            new Room
            {
                Id = "1",
                Beds = 2,
                KingBeds = 1,
                QueenBeds = 0,
                TwinBeds = 1,
                Bathroom = true,
                Bathtub = false,
                Size = 22,
                WiFi = true,
                Fridge = true,
                Stove = false,
                Oven = false,
                Microwave = false,
                Tv = 1,
                Clean = true,
                Price = 899.00,
                Description = "Standard twin room with modern amenities.",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Room
            {
                Id = "2",
                Beds = 1,
                KingBeds = 1,
                QueenBeds = 0,
                TwinBeds = 0,
                Bathroom = true,
                Bathtub = true,
                Size = 28,
                WiFi = false,
                Fridge = true,
                Stove = false,
                Oven = false,
                Microwave = true,
                Tv = 1,
                Clean = true,
                Price = 1199.00,
                Description = "King suite with bathtub and microwave.",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Room
            {
                Id = "3",
                Beds = 3,
                KingBeds = 0,
                QueenBeds = 2,
                TwinBeds = 1,
                Bathroom = false,
                Bathtub = true,
                Size = 42,
                WiFi = true,
                Fridge = true,
                Stove = true,
                Oven = true,
                Microwave = true,
                Tv = 2,
                Clean = false,
                Price = 1799.00,
                Description = "Family room with kitchen and two bathrooms.",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Room
            {
                Id = "4",
                Beds = 1,
                KingBeds = 0,
                QueenBeds = 1,
                TwinBeds = 0,
                Bathroom = true,
                Bathtub = false,
                Size = 18,
                WiFi = false,
                Fridge = false,
                Stove = false,
                Oven = false,
                Microwave = false,
                Tv = 0,
                Clean = true,
                Price = 699.00,
                Description = "Budget room, basic accommodation for one or two guests.",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        modelBuilder.Entity<Room>().HasData(rooms);
        modelBuilder.Entity<Role>().HasData(roles);
    }
}