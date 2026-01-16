using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Garage_3._0.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingSpots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpotNumber",
                table: "ParkingSpots",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpotNumber",
                table: "ParkingSpots");
        }
    }
}
