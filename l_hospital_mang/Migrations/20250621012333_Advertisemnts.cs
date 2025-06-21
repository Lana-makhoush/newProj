using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace l_hospital_mang.Migrations
{
    /// <inheritdoc />
    public partial class Advertisemnts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Advertismentss_ClinicId",
                table: "Advertismentss");

            migrationBuilder.AlterColumn<long>(
                name: "ClinicId",
                table: "Advertismentss",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Advertismentss_ClinicId",
                table: "Advertismentss",
                column: "ClinicId",
                unique: true,
                filter: "[ClinicId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Advertismentss_ClinicId",
                table: "Advertismentss");

            migrationBuilder.AlterColumn<long>(
                name: "ClinicId",
                table: "Advertismentss",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Advertismentss_ClinicId",
                table: "Advertismentss",
                column: "ClinicId",
                unique: true);
        }
    }
}
