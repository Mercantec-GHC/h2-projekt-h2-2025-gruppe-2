using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class rooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Room",
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
                    table.PrimaryKey("PK_Room", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Room");
        }
    }
}
