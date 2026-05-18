using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutineDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysCount",
                table: "Routines",
                
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsByDays",
                table: "Routines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DayNumber",
                table: "RoutineExercises",
                
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysCount",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "IsByDays",
                table: "Routines");

            migrationBuilder.DropColumn(
                name: "DayNumber",
                table: "RoutineExercises");
        }
    }
}
