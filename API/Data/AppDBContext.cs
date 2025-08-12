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
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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

        modelBuilder.Entity<Role>().HasData(roles);
    }
}