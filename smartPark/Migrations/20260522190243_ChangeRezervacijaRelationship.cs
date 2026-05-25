using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartPark.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRezervacijaRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rezervacije_ParkingMjestoId",
                table: "Rezervacije");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_ParkingMjestoId",
                table: "Rezervacije",
                column: "ParkingMjestoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rezervacije_ParkingMjestoId",
                table: "Rezervacije");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_ParkingMjestoId",
                table: "Rezervacije",
                column: "ParkingMjestoId",
                unique: true,
                filter: "[ParkingMjestoId] IS NOT NULL");
        }
    }
}
