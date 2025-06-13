using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace l_hospital_mang.Migrations
{
    /// <inheritdoc />
    public partial class Doctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctorss_AspNetUsers_UserId",
                table: "Doctorss");

            migrationBuilder.DropIndex(
                name: "IX_Doctorss_UserId",
                table: "Doctorss");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Doctorss");

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Doctorss",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Doctorss");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Doctorss",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctorss_UserId",
                table: "Doctorss",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctorss_AspNetUsers_UserId",
                table: "Doctorss",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
