using System;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260519093000_AddTerminalAttendance")]
    public partial class AddTerminalAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IngresosUtilizados",
                table: "Memberships",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Ingresos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(nullable: false),
                    AlumnoId = table.Column<int>(nullable: false),
                    TerminalId = table.Column<int>(nullable: false),
                    MembershipId = table.Column<int>(nullable: false),
                    FechaHora = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingresos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingresos_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingresos_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingresos_Users_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingresos_Users_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_AlumnoId",
                table: "Ingresos",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_FechaHora",
                table: "Ingresos",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_GymId_AlumnoId",
                table: "Ingresos",
                columns: new[] { "GymId", "AlumnoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_MembershipId",
                table: "Ingresos",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_TerminalId",
                table: "Ingresos",
                column: "TerminalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingresos");

            migrationBuilder.DropColumn(
                name: "IngresosUtilizados",
                table: "Memberships");
        }
    }
}
