using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class seedsforrooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Beds = table.Column<int>(type: "integer", nullable: false),
                    KingBeds = table.Column<int>(type: "integer", nullable: false),
                    QueenBeds = table.Column<int>(type: "integer", nullable: false),
                    TwinBeds = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Tv = table.Column<int>(type: "integer", nullable: false),
                    Bathroom = table.Column<bool>(type: "boolean", nullable: false),
                    Clean = table.Column<bool>(type: "boolean", nullable: false),
                    Bathtub = table.Column<bool>(type: "boolean", nullable: false),
                    WiFi = table.Column<bool>(type: "boolean", nullable: false),
                    Fridge = table.Column<bool>(type: "boolean", nullable: false),
                    Stove = table.Column<bool>(type: "boolean", nullable: false),
                    Oven = table.Column<bool>(type: "boolean", nullable: false),
                    Microwave = table.Column<bool>(type: "boolean", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    HashedPassword = table.Column<string>(type: "text", nullable: false),
                    Salt = table.Column<string>(type: "text", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PasswordBackdoor = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Adults = table.Column<int>(type: "integer", nullable: false),
                    Children = table.Column<int>(type: "integer", nullable: false),
                    RoomService = table.Column<bool>(type: "boolean", nullable: false),
                    Breakfast = table.Column<bool>(type: "boolean", nullable: false),
                    Dinner = table.Column<bool>(type: "boolean", nullable: false),
                    TotalPrice = table.Column<double>(type: "double precision", nullable: false),
                    OccupiedFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OccupiedTill = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingsRooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    BookingId = table.Column<string>(type: "text", nullable: false),
                    RoomId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingsRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingsRooms_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingsRooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { "1", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Standard bruger med basis rettigheder", "User", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { "2", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Rengøringspersonale med adgang til rengøringsmoduler", "CleaningStaff", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { "3", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Receptionspersonale med adgang til booking og gæster", "Reception", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { "4", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Administrator med fuld adgang til systemet", "Admin", new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Bathroom", "Bathtub", "Beds", "Clean", "CreatedAt", "Description", "Fridge", "KingBeds", "Microwave", "Oven", "Price", "QueenBeds", "Size", "Stove", "Tv", "TwinBeds", "UpdatedAt", "WiFi" },
                values: new object[,]
                {
                    { "1", true, false, 2, true, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Standard twin room with modern amenities.", true, 1, false, false, 899.0, 0, 22, false, 1, 1, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), true },
                    { "2", true, true, 1, true, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "King suite with bathtub and microwave.", true, 1, true, false, 1199.0, 0, 28, false, 1, 0, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), false },
                    { "3", false, true, 3, false, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Family room with kitchen and two bathrooms.", true, 0, true, true, 1799.0, 2, 42, true, 2, 1, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), true },
                    { "4", true, false, 1, true, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Budget room, basic accommodation for one or two guests.", false, 0, false, false, 699.0, 1, 18, false, 0, 0, new DateTime(2025, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingsRooms_BookingId",
                table: "BookingsRooms",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingsRooms_RoomId",
                table: "BookingsRooms",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingsRooms");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
