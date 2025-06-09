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
                name: "FK_Medical_Healths_Patients_PatientId",
                table: "Medical_Healths");

            migrationBuilder.DropIndex(
                name: "IX_Medical_Healths_PatientId",
                table: "Medical_Healths");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Healths_PatientId",
                table: "Medical_Healths",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medical_Healths_Patients_PatientId",
                table: "Medical_Healths",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medical_Healths_Patients_PatientId",
                table: "Medical_Healths");

            migrationBuilder.DropIndex(
                name: "IX_Medical_Healths_PatientId",
                table: "Medical_Healths");

            migrationBuilder.CreateIndex(
                name: "IX_Medical_Healths_PatientId",
                table: "Medical_Healths",
                column: "PatientId",
                unique: true,
                filter: "[PatientId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Medical_Healths_Patients_PatientId",
                table: "Medical_Healths",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
