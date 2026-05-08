using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    public partial class AddRoutineExerciseBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bloque",
                table: "RoutineExercises",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "parteMedia");

            migrationBuilder.Sql("""
                UPDATE "RoutineExercises"
                SET "Bloque" = CASE
                    WHEN "Orden" = 1 THEN 'calentamientoInicial'
                    WHEN "Orden" = 2 THEN 'parteMedia'
                    ELSE 'fuerza'
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bloque",
                table: "RoutineExercises");
        }
    }
}
