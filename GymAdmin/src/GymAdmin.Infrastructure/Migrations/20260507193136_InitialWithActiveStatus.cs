using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithActiveStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    GrupoMuscular = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    GoogleId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Alumno"),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RefreshToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RefreshTokenExpiracion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Routines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ProfesorId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routines_Users_ProfesorId",
                        column: x => x.ProfesorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoutineExercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RutinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EjercicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Series = table.Column<int>(type: "INTEGER", nullable: false),
                    Repeticiones = table.Column<int>(type: "INTEGER", nullable: false),
                    Peso = table.Column<decimal>(type: "TEXT", precision: 8, scale: 2, nullable: true),
                    DescansoSegundos = table.Column<int>(type: "INTEGER", nullable: true),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlumnoId = table.Column<int>(type: "INTEGER", nullable: false),
                    RutinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRoutines", x => x.Id);
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
                name: "IX_RoutineExercises_EjercicioId",
                table: "RoutineExercises",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutineExercises_RutinaId_Orden",
                table: "RoutineExercises",
                columns: new[] { "RutinaId", "Orden" },
                unique: true);

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
                name: "IX_StudentRoutines_RutinaId",
                table: "StudentRoutines",
                column: "RutinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoutineExercises");

            migrationBuilder.DropTable(
                name: "StudentRoutines");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Routines");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
