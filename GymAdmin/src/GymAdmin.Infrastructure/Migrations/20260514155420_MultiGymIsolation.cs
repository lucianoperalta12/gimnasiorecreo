using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiGymIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Users",
                
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "Users",
                
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "Users",
                
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "StudentRoutines",
                
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "Routines",
                
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "GymId",
                table: "Exercises",
                
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Gyms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(maxLength: 150, nullable: false),
                    DuenoNombreApellido = table.Column<string>(maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(maxLength: 500, nullable: true),
                    ColorPrincipalHex = table.Column<string>(maxLength: 7, nullable: false),
                    Activo = table.Column<bool>(nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gyms", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Gyms" ("Id", "Nombre", "DuenoNombreApellido", "LogoUrl", "ColorPrincipalHex", "Activo", "FechaCreacion")
                VALUES (1, 'Gimnasio Central', 'Administrador General', NULL, '#2563EB', true, CURRENT_TIMESTAMP);
                """);

            migrationBuilder.Sql("""
                UPDATE "Users"
                SET
                    "GymId" = 1,
                    "Apellido" = CASE WHEN "Apellido" = '' THEN "Nombre" ELSE "Apellido" END,
                    "Dni" = CASE WHEN "Dni" = '' THEN "Email" ELSE "Dni" END,
                    "DebeCambiarPassword" = false;
                """);

            migrationBuilder.Sql("UPDATE \"Exercises\" SET \"GymId\" = 1;");
            migrationBuilder.Sql("UPDATE \"Routines\" SET \"GymId\" = 1;");
            migrationBuilder.Sql("UPDATE \"StudentRoutines\" SET \"GymId\" = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Dni",
                table: "Users",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GymId",
                table: "Users",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRoutines_GymId",
                table: "StudentRoutines",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_Routines_GymId",
                table: "Routines",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_GymId",
                table: "Exercises",
                column: "GymId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Gyms_GymId",
                table: "Exercises",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Routines_Gyms_GymId",
                table: "Routines",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRoutines_Gyms_GymId",
                table: "StudentRoutines",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Gyms_GymId",
                table: "Users",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Gyms_GymId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Routines_Gyms_GymId",
                table: "Routines");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRoutines_Gyms_GymId",
                table: "StudentRoutines");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Gyms_GymId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Gyms");

            migrationBuilder.DropIndex(
                name: "IX_Users_Dni",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GymId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_StudentRoutines_GymId",
                table: "StudentRoutines");

            migrationBuilder.DropIndex(
                name: "IX_Routines_GymId",
                table: "Routines");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_GymId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Dni",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "StudentRoutines");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "GymId",
                table: "Exercises");
        }
    }
}
