using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingBuildingManagementSystem_BE.Migrations
{
    /// <inheritdoc />
    public partial class FixQrCodeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__bookings__E2FB88893F98E923",
                table: "bookings");

            migrationBuilder.CreateIndex(
                name: "UQ__bookings__E2FB88893F98E923",
                table: "bookings",
                column: "qr_code",
                unique: true,
                filter: "[qr_code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ__bookings__E2FB88893F98E923",
                table: "bookings");

            migrationBuilder.CreateIndex(
                name: "UQ__bookings__E2FB88893F98E923",
                table: "bookings",
                column: "qr_code",
                unique: true);
        }
    }
}
