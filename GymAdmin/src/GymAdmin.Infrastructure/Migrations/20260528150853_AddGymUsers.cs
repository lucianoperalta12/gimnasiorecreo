using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGymUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GymUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GymId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    FechaAsociacion = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymUsers_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GymUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""GymUsers"" (""GymId"", ""UserId"", ""Rol"", ""Activo"", ""FechaAsociacion"")
                SELECT u.""GymId"", u.""Id"", u.""Rol"", u.""Activo"", COALESCE(u.""FechaCreacion"", CURRENT_TIMESTAMP)
                FROM ""Users"" u
                WHERE u.""GymId"" IS NOT NULL
                  AND u.""GymId"" > 0
                  AND NOT EXISTS (
                      SELECT 1 FROM ""GymUsers"" gu
                      WHERE gu.""GymId"" = u.""GymId"" AND gu.""UserId"" = u.""Id""
                  );");

            migrationBuilder.CreateIndex(
                name: "IX_GymUsers_GymId_UserId",
                table: "GymUsers",
                columns: new[] { "GymId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GymUsers_UserId",
                table: "GymUsers",
                column: "UserId");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Gyms_GymId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GymId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Users",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Alumno");

            migrationBuilder.Sql(@"
                UPDATE ""Users""
                SET ""GymId"" = (
                    SELECT gu.""GymId"" FROM ""GymUsers"" gu
                    WHERE gu.""UserId"" = ""Users"".""Id"" AND gu.""Activo"" = 1
                    ORDER BY gu.""FechaAsociacion"" DESC
                    LIMIT 1
                ),
                ""Rol"" = (
                    SELECT gu.""Rol"" FROM ""GymUsers"" gu
                    WHERE gu.""UserId"" = ""Users"".""Id"" AND gu.""Activo"" = 1
                    ORDER BY gu.""FechaAsociacion"" DESC
                    LIMIT 1
                );");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GymId",
                table: "Users",
                column: "GymId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Gyms_GymId",
                table: "Users",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(
                name: "GymUsers");
        }
    }
}
