using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartPark.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePricelistsAndDefaultParkingPricelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipPerioda",
                table: "Cjenovnici");

            migrationBuilder.RenameColumn(
                name: "CijenaPoSatu",
                table: "Cjenovnici",
                newName: "CijenaNocna");

            migrationBuilder.AddColumn<int>(
                name: "DefaultniCjenovnikId",
                table: "Parkinzi",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CijenaDnevna",
                table: "Cjenovnici",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Parkinzi_DefaultniCjenovnikId",
                table: "Parkinzi",
                column: "DefaultniCjenovnikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parkinzi_Cjenovnici_DefaultniCjenovnikId",
                table: "Parkinzi",
                column: "DefaultniCjenovnikId",
                principalTable: "Cjenovnici",
                principalColumn: "CjenovnikId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parkinzi_Cjenovnici_DefaultniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropIndex(
                name: "IX_Parkinzi_DefaultniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropColumn(
                name: "DefaultniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropColumn(
                name: "CijenaDnevna",
                table: "Cjenovnici");

            migrationBuilder.RenameColumn(
                name: "CijenaNocna",
                table: "Cjenovnici",
                newName: "CijenaPoSatu");

            migrationBuilder.AddColumn<int>(
                name: "TipPerioda",
                table: "Cjenovnici",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
