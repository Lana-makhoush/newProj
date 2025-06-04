using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace l_hospital_mang.Migrations
{
    /// <inheritdoc />
    public partial class Resident_patients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resident_patientss_RoomId",
                table: "Resident_patientss");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Resident_patientss",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Resident_patientss_RoomId",
                table: "Resident_patientss",
                column: "RoomId",
                unique: true,
                filter: "[RoomId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resident_patientss_RoomId",
                table: "Resident_patientss");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Resident_patientss",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resident_patientss_RoomId",
                table: "Resident_patientss",
                column: "RoomId",
                unique: true);
        }
    }
}
