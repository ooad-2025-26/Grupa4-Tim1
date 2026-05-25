using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartPark.Migrations
{
    /// <inheritdoc />
    public partial class AddRezervacijaPodsjetnikFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IstekPodsjetnikPoslan",
                table: "Rezervacije",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PocetakPodsjetnikPoslan",
                table: "Rezervacije",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IstekPodsjetnikPoslan",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "PocetakPodsjetnikPoslan",
                table: "Rezervacije");
        }
    }
}
