using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDeleteRoutines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routines_Users_ProfesorId",
                table: "Routines");

            migrationBuilder.AddForeignKey(
                name: "FK_Routines_Users_ProfesorId",
                table: "Routines",
                column: "ProfesorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routines_Users_ProfesorId",
                table: "Routines");

            migrationBuilder.AddForeignKey(
                name: "FK_Routines_Users_ProfesorId",
                table: "Routines",
                column: "ProfesorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
