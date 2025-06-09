using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace l_hospital_mang.Migrations
{
    /// <inheritdoc />
    public partial class patient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "surgery_reservations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    DoctorId = table.Column<long>(type: "bigint", nullable: false),
                    SurgeryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SurgeryType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surgery_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_surgery_reservations_Doctorss_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctorss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_surgery_reservations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_surgery_reservations_DoctorId",
                table: "surgery_reservations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_surgery_reservations_PatientId",
                table: "surgery_reservations",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "surgery_reservations");
        }
    }
}
