using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymAdmin.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        // Ensure super user exists and has correct password
        var superUser = await context.Users.FirstOrDefaultAsync(u => u.Nombre == "admin" || u.Email == "admin");
        if (superUser == null)
        {
            superUser = new User
            {
                Nombre = "admin",
                Email = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Rol = UserRole.Superusuario,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Users.Add(superUser);
        }
        else
        {
            superUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin");
            superUser.Activo = true;
            context.Users.Update(superUser);
        }

        // Ensure professor exists
        var profesor = await context.Users.FirstOrDefaultAsync(u => u.Nombre == "profesor" || u.Email == "profesor");
        if (profesor == null)
        {
            profesor = new User
            {
                Nombre = "profesor",
                Email = "profesor",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("profesor"),
                Rol = UserRole.Profesor,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Users.Add(profesor);
        }
        else
        {
            profesor.PasswordHash = BCrypt.Net.BCrypt.HashPassword("profesor");
            profesor.Activo = true;
            context.Users.Update(profesor);
        }

        // Ensure student exists
        var alumno = await context.Users.FirstOrDefaultAsync(u => u.Nombre == "alumno" || u.Email == "alumno");
        if (alumno == null)
        {
            alumno = new User
            {
                Nombre = "alumno",
                Email = "alumno",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("alumno"),
                Rol = UserRole.Alumno,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Users.Add(alumno);
        }
        else
        {
            alumno.PasswordHash = BCrypt.Net.BCrypt.HashPassword("alumno");
            alumno.Activo = true;
            context.Users.Update(alumno);
        }

        // Ensure personal user exists
        var personal = await context.Users.FirstOrDefaultAsync(u => u.Email == "lucianoperalta12@gmail.com");
        if (personal == null)
        {
            personal = new User
            {
                Nombre = "Luciano Peralta",
                Email = "lucianoperalta12@gmail.com",
                Rol = UserRole.Superusuario,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Users.Add(personal);
        }
        else
        {
            personal.Activo = true;
            context.Users.Update(personal);
        }

        // Seed more students for testing
        var moreStudents = new List<(string Name, string Email)>
        {
            ("Ana García", "ana.garcia@test.com"),
            ("Beto Casella", "beto.casella@test.com"),
            ("Carlos Martínez", "carlos.mtz@test.com"),
            ("Diana Prince", "diana.prince@test.com"),
            ("Eduardo Galeano", "eduardo.g@test.com"),
            ("Facundo Cabral", "facundo.c@test.com"),
            ("Gisela Dulko", "gisela.d@test.com"),
            ("Horacio Pagani", "horacio.p@test.com"),
            ("Ivana Nadal", "ivana.n@test.com"),
            ("Juan Román Riquelme", "jrr@test.com"),
            ("Karina La Princesita", "karina@test.com"),
            ("Lionel Messi", "leo.messi@test.com")
        };

        foreach (var (name, email) in moreStudents)
        {
            if (!await context.Users.AnyAsync(u => u.Email == email))
            {
                context.Users.Add(new User
                {
                    Nombre = name,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Rol = UserRole.Alumno,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();

        // Seed exercises if none exist
        if (!await context.Exercises.AnyAsync())
        {
            var exercises = new List<Exercise>
            {
                new() { Nombre = "Press de banca", Descripcion = "Ejercicio compuesto para pecho", GrupoMuscular = "Pecho" },
                new() { Nombre = "Sentadilla", Descripcion = "Ejercicio compuesto para piernas", GrupoMuscular = "Piernas" },
                new() { Nombre = "Peso muerto", Descripcion = "Ejercicio compuesto para espalda baja y piernas", GrupoMuscular = "Espalda" },
                new() { Nombre = "Press militar", Descripcion = "Ejercicio compuesto para hombros", GrupoMuscular = "Hombros" },
                new() { Nombre = "Curl de bíceps", Descripcion = "Ejercicio de aislamiento para bíceps", GrupoMuscular = "Bíceps" },
                new() { Nombre = "Extensión de tríceps", Descripcion = "Ejercicio de aislamiento para tríceps", GrupoMuscular = "Tríceps" },
                new() { Nombre = "Dominadas", Descripcion = "Ejercicio compuesto para espalda", GrupoMuscular = "Espalda" },
                new() { Nombre = "Remo con barra", Descripcion = "Ejercicio compuesto para espalda media", GrupoMuscular = "Espalda" },
                new() { Nombre = "Elevaciones laterales", Descripcion = "Ejercicio de aislamiento para deltoides lateral", GrupoMuscular = "Hombros" },
                new() { Nombre = "Prensa de piernas", Descripcion = "Ejercicio compuesto para cuádriceps", GrupoMuscular = "Piernas" },
                new() { Nombre = "Curl femoral", Descripcion = "Ejercicio de aislamiento para isquiotibiales", GrupoMuscular = "Piernas" },
                new() { Nombre = "Plancha", Descripcion = "Ejercicio isométrico para core", GrupoMuscular = "Abdominales" },
            };
            context.Exercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }

        // Seed basic routines if they don't exist
        var basicRoutines = new List<(string Name, string Desc)>
        {
            ("Rutina Full Body Principiante", "Ideal para comenzar tu entrenamiento con ejercicios compuestos."),
            ("Tren Inferior y Glúteos", "Enfoque en fuerza de piernas y estabilidad.")
        };

        foreach (var (name, desc) in basicRoutines)
        {
            if (!await context.Routines.AnyAsync(r => r.Nombre == name))
            {
                context.Routines.Add(new Routine
                {
                    Nombre = name,
                    Descripcion = desc,
                    ProfesorId = profesor.Id,
                    Activa = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        // Seed more routines for testing
        var moreRoutines = new List<(string Name, string Desc)>
        {
            ("Hipertrofia Pecho y Espalda", "Enfoque en volumen muscular para el torso."),
            ("Acondicionamiento Físico", "Rutina de alta intensidad y poco descanso."),
            ("Powerlifting Iniciación", "Enfoque en los tres grandes: Sentadilla, Banco y Peso Muerto."),
            ("Movilidad y Flexibilidad", "Ideal para días de recuperación activa."),
            ("Fuerza de Hombros", "Enfoque en deltoides y trapecio."),
            ("Rutina Express 30 min", "Para cuando tenés poco tiempo pero querés entrenar.")
        };

        foreach (var (name, desc) in moreRoutines)
        {
            if (!await context.Routines.AnyAsync(r => r.Nombre == name))
            {
                context.Routines.Add(new Routine
                {
                    Nombre = name,
                    Descripcion = desc,
                    ProfesorId = profesor.Id,
                    Activa = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
