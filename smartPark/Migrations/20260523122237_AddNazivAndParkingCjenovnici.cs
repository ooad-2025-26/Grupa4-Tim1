using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartPark.Migrations
{
    /// <inheritdoc />
    public partial class AddNazivAndParkingCjenovnici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cjenovnici_Parkinzi_ParkingId",
                table: "Cjenovnici");

            migrationBuilder.DropIndex(
                name: "IX_Parkinzi_MenadzerID",
                table: "Parkinzi");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MenadzerOdgovorniParkingId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "DnevniCjenovnikId",
                table: "Parkinzi",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NocniCjenovnikId",
                table: "Parkinzi",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ParkingId",
                table: "Cjenovnici",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Naziv",
                table: "Cjenovnici",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Parkinzi_DnevniCjenovnikId",
                table: "Parkinzi",
                column: "DnevniCjenovnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Parkinzi_MenadzerID",
                table: "Parkinzi",
                column: "MenadzerID");

            migrationBuilder.CreateIndex(
                name: "IX_Parkinzi_NocniCjenovnikId",
                table: "Parkinzi",
                column: "NocniCjenovnikId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MenadzerOdgovorniParkingId",
                table: "AspNetUsers",
                column: "MenadzerOdgovorniParkingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cjenovnici_Parkinzi_ParkingId",
                table: "Cjenovnici",
                column: "ParkingId",
                principalTable: "Parkinzi",
                principalColumn: "ParkingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parkinzi_Cjenovnici_DnevniCjenovnikId",
                table: "Parkinzi",
                column: "DnevniCjenovnikId",
                principalTable: "Cjenovnici",
                principalColumn: "CjenovnikId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parkinzi_Cjenovnici_NocniCjenovnikId",
                table: "Parkinzi",
                column: "NocniCjenovnikId",
                principalTable: "Cjenovnici",
                principalColumn: "CjenovnikId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cjenovnici_Parkinzi_ParkingId",
                table: "Cjenovnici");

            migrationBuilder.DropForeignKey(
                name: "FK_Parkinzi_Cjenovnici_DnevniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropForeignKey(
                name: "FK_Parkinzi_Cjenovnici_NocniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropIndex(
                name: "IX_Parkinzi_DnevniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropIndex(
                name: "IX_Parkinzi_MenadzerID",
                table: "Parkinzi");

            migrationBuilder.DropIndex(
                name: "IX_Parkinzi_NocniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MenadzerOdgovorniParkingId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DnevniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropColumn(
                name: "NocniCjenovnikId",
                table: "Parkinzi");

            migrationBuilder.DropColumn(
                name: "Naziv",
                table: "Cjenovnici");

            migrationBuilder.AlterColumn<int>(
                name: "ParkingId",
                table: "Cjenovnici",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parkinzi_MenadzerID",
                table: "Parkinzi",
                column: "MenadzerID",
                unique: true,
                filter: "[MenadzerID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MenadzerOdgovorniParkingId",
                table: "AspNetUsers",
                column: "MenadzerOdgovorniParkingId",
                unique: true,
                filter: "[MenadzerOdgovorniParkingId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Cjenovnici_Parkinzi_ParkingId",
                table: "Cjenovnici",
                column: "ParkingId",
                principalTable: "Parkinzi",
                principalColumn: "ParkingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
