using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPayments_Memberships_MembresiaId",
                table: "MembershipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_AlumnoId",
                table: "Memberships");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MembershipPayments",
                
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldDefaultValue: "Completado");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPayments_Memberships_MembresiaId",
                table: "MembershipPayments",
                column: "MembresiaId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_AlumnoId",
                table: "Memberships",
                column: "AlumnoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipPayments_Memberships_MembresiaId",
                table: "MembershipPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Users_AlumnoId",
                table: "Memberships");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MembershipPayments",
                
                maxLength: 20,
                nullable: false,
                defaultValue: "Completado",
                oldClrType: typeof(string),
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipPayments_Memberships_MembresiaId",
                table: "MembershipPayments",
                column: "MembresiaId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Users_AlumnoId",
                table: "Memberships",
                column: "AlumnoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
