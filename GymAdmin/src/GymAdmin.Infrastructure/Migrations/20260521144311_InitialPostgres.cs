using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gyms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DuenoNombreApellido = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ColorPrincipalHex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Moneda = table.Column<string>(type: "text", nullable: false),
                    VeRutinas = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gyms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GrupoMuscular = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DuracionDias = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PaseLibre = table.Column<bool>(type: "boolean", nullable: false),
                    DiasPorSemana = table.Column<int>(type: "integer", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Dni = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    GoogleId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Alumno"),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "boolean", nullable: false),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    AlumnoId = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Activa"),
                    IngresosUtilizados = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProfesorId = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsByDays = table.Column<bool>(type: "boolean", nullable: false),
                    DaysCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routines_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Routines_Users_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingresos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    AlumnoId = table.Column<int>(type: "integer", nullable: false),
                    TerminalId = table.Column<int>(type: "integer", nullable: false),
                    MembershipId = table.Column<int>(type: "integer", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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

            migrationBuilder.CreateTable(
                name: "MembershipPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    MembresiaId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutineExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RutinaId = table.Column<int>(type: "integer", nullable: false),
                    EjercicioId = table.Column<int>(type: "integer", nullable: false),
                    Bloque = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Series = table.Column<int>(type: "integer", nullable: false),
                    Repeticiones = table.Column<int>(type: "integer", nullable: false),
                    Peso = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    DescansoSegundos = table.Column<int>(type: "integer", nullable: true),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DayNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutineExercises_Exercises_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoutineExercises_Routines_RutinaId",
                        column: x => x.RutinaId,
                        principalTable: "Routines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentRoutines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GymId = table.Column<int>(type: "integer", nullable: false),
                    AlumnoId = table.Column<int>(type: "integer", nullable: false),
                    RutinaId = table.Column<int>(type: "integer", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRoutines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentRoutines_Gyms_GymId",
                        column: x => x.GymId,
                        principalTable: "Gyms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentRoutines_Routines_RutinaId",
                        column: x => x.RutinaId,
                        principalTable: "Routines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentRoutines_Users_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_GymId",
                table: "Exercises",
                column: "GymId");

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

            migrationBuilder.CreateIndex(
                name: "IX_RoutineExercises_EjercicioId",
                table: "RoutineExercises",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineExercises_RutinaId_Orden",
                table: "RoutineExercises",
                columns: new[] { "RutinaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routines_GymId",
                table: "Routines",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_Routines_ProfesorId",
                table: "Routines",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRoutines_AlumnoId_RutinaId",
                table: "StudentRoutines",
                columns: new[] { "AlumnoId", "RutinaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRoutines_GymId",
                table: "StudentRoutines",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRoutines_RutinaId",
                table: "StudentRoutines",
                column: "RutinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Dni",
                table: "Users",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GymId",
                table: "Users",
                column: "GymId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingresos");

            migrationBuilder.DropTable(
                name: "MembershipPayments");

            migrationBuilder.DropTable(
                name: "RoutineExercises");

            migrationBuilder.DropTable(
                name: "StudentRoutines");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Routines");

            migrationBuilder.DropTable(
                name: "MembershipPlans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Gyms");
        }
    }
}
