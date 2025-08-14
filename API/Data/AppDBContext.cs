using DomainModels;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<BookingsRooms> BookingsRooms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfigurer Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            // Navn skal være unikt
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // Konfigurer User entity
        modelBuilder.Entity<User>(entity =>
        {
            // Email skal være unikt
            entity.HasIndex(u => u.Email).IsUnique();

            // Konfigurer foreign key til Role
            entity.HasOne(u => u.Roles)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-one relation med bookings
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

        // Seed roller og test brugere (kun til udvikling)
        SeedRoles(modelBuilder);
    }

    private void SeedRoles(ModelBuilder modelBuilder)
    {
        var roles = new[]
        {
            new Role
            {
                // Nyt tilfældigt guid
                Id = "1",
                Name = "User",
                Description = "Standard bruger med basis rettigheder",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "2",
                Name = "CleaningStaff",
                Description = "Rengøringspersonale med adgang til rengøringsmoduler",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "3",
                Name = "Reception",
                Description = "Receptionspersonale med adgang til booking og gæster",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = "4",
                Name = "Admin",
                Description = "Administrator med fuld adgang til systemet",
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