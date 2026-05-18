using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MembershipPlans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(nullable: false),
                    Nombre = table.Column<string>(maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(maxLength: 1000, nullable: true),
                    DuracionDias = table.Column<int>(nullable: false),
                    Precio = table.Column<decimal>(nullable: false),
                    Activo = table.Column<bool>(nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipPlans_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(nullable: false),
                    AlumnoId = table.Column<int>(nullable: false),
                    PlanId = table.Column<int>(nullable: false),
                    FechaInicio = table.Column<DateTime>(nullable: false),
                    FechaVencimiento = table.Column<DateTime>(nullable: false),
                    Estado = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "Activa"),
                    Notas = table.Column<string>(maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Memberships_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_MembershipPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_Users_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPayments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true).Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(nullable: false),
                    MembresiaId = table.Column<int>(nullable: false),
                    Monto = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                    FechaPago = table.Column<DateTime>(nullable: false),
                    MetodoPago = table.Column<string>(maxLength: 50, nullable: true),
                    Estado = table.Column<string>(maxLength: 20, nullable: false, defaultValue: "Completado"),
                    Referencia = table.Column<string>(maxLength: 200, nullable: true),
                    Notas = table.Column<string>(maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipPayments_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipPayments_Memberships_MembresiaId",
                        column: x => x.MembresiaId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPayments_FechaPago",
                table: "MembershipPayments",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPayments_GymId_MembresiaId",
                table: "MembershipPayments",
                columns: new[] { "GymId", "MembresiaId" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPayments_MembresiaId",
                table: "MembershipPayments",
                column: "MembresiaId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_GymId_Nombre",
                table: "MembershipPlans",
                columns: new[] { "GymId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_AlumnoId",
                table: "Memberships",
                column: "AlumnoId",
                unique: true,
                filter: "\"Estado\" = 'Activa'");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_Estado",
                table: "Memberships",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_FechaVencimiento",
                table: "Memberships",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_GymId_AlumnoId",
                table: "Memberships",
                columns: new[] { "GymId", "AlumnoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_PlanId",
                table: "Memberships",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipPayments");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "MembershipPlans");
        }
    }
}
