using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGymVeRutinas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasPorSemana",
                table: "MembershipPlans",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaseLibre",
                table: "MembershipPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VeRutinas",
                table: "Gyms",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasPorSemana",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "PaseLibre",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "VeRutinas",
                table: "Gyms");
        }
    }
}
