using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace l_hospital_mang.Migrations
{
    /// <inheritdoc />
    public partial class Medical_Health : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_adiographyies_Medical_Healths_Medical_HealthId",
                table: "adiographyies");

            migrationBuilder.DropIndex(
                name: "IX_adiographyies_Medical_HealthId",
                table: "adiographyies");

            migrationBuilder.DropColumn(
                name: "Medical_HealthId",
                table: "adiographyies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Medical_HealthId",
                table: "adiographyies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_adiographyies_Medical_HealthId",
                table: "adiographyies",
                column: "Medical_HealthId");

            migrationBuilder.AddForeignKey(
                name: "FK_adiographyies_Medical_Healths_Medical_HealthId",
                table: "adiographyies",
                column: "Medical_HealthId",
                principalTable: "Medical_Healths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
