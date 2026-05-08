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

        await context.SaveChangesAsync();

        if (await context.Exercises.AnyAsync())
            return;

        // Seed exercises
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

        // Add video URLs to some exercises for demonstration
        exercises[0].VideoUrl = "https://www.youtube.com/watch?v=tuwHzzPrzSA"; // Bench Press
        exercises[1].VideoUrl = "https://www.youtube.com/watch?v=UXJrBgI2RxM"; // Squat
        exercises[6].VideoUrl = "https://www.youtube.com/watch?v=eGo4IYlbE5g"; // Pull-ups

        context.Exercises.UpdateRange(exercises);
        await context.SaveChangesAsync();

        // Seed a routine
        var rutina1 = new Routine
        {
            Nombre = "Rutina Full Body Principiante",
            Descripcion = "Ideal para comenzar tu entrenamiento con ejercicios compuestos.",
            ProfesorId = profesor.Id,
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        var rutina2 = new Routine
        {
            Nombre = "Tren Inferior y Glúteos",
            Descripcion = "Enfoque en fuerza de piernas y estabilidad.",
            ProfesorId = profesor.Id,
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        context.Routines.AddRange(rutina1, rutina2);
        await context.SaveChangesAsync();

        // Add exercises to routine 1
        var re1 = new List<RoutineExercise>
        {
            new() { RutinaId = rutina1.Id, EjercicioId = exercises[0].Id, Bloque = RoutineExerciseSectionLabels.CalentamientoInicial, Series = 3, Repeticiones = 10, Peso = 40, DescansoSegundos = 90, Orden = 1 },
            new() { RutinaId = rutina1.Id, EjercicioId = exercises[1].Id, Bloque = RoutineExerciseSectionLabels.ParteMedia, Series = 4, Repeticiones = 8, Peso = 50, DescansoSegundos = 120, Orden = 2 },
            new() { RutinaId = rutina1.Id, EjercicioId = exercises[6].Id, Bloque = RoutineExerciseSectionLabels.Fuerza, Series = 3, Repeticiones = 8, DescansoSegundos = 90, Orden = 3 },
        };

        // Add exercises to routine 2
        var re2 = new List<RoutineExercise>
        {
            new() { RutinaId = rutina2.Id, EjercicioId = exercises[1].Id, Bloque = RoutineExerciseSectionLabels.CalentamientoInicial, Series = 4, Repeticiones = 10, Peso = 60, DescansoSegundos = 120, Orden = 1 },
            new() { RutinaId = rutina2.Id, EjercicioId = exercises[9].Id, Bloque = RoutineExerciseSectionLabels.ParteMedia, Series = 3, Repeticiones = 12, Peso = 80, DescansoSegundos = 90, Orden = 2 },
            new() { RutinaId = rutina2.Id, EjercicioId = exercises[10].Id, Bloque = RoutineExerciseSectionLabels.Fuerza, Series = 3, Repeticiones = 15, Peso = 30, DescansoSegundos = 60, Orden = 3 },
        };

        context.RoutineExercises.AddRange(re1);
        context.RoutineExercises.AddRange(re2);
        await context.SaveChangesAsync();

        // Assign routines to students
        var assignments = new List<StudentRoutine>
        {
            new() { AlumnoId = alumno.Id, RutinaId = rutina1.Id, FechaAsignacion = DateTime.UtcNow, Activa = true },
            new() { AlumnoId = alumno.Id, RutinaId = rutina2.Id, FechaAsignacion = DateTime.UtcNow, Activa = true },
            new() { AlumnoId = personal.Id, RutinaId = rutina1.Id, FechaAsignacion = DateTime.UtcNow, Activa = true },
            new() { AlumnoId = personal.Id, RutinaId = rutina2.Id, FechaAsignacion = DateTime.UtcNow, Activa = true }
        };

        context.StudentRoutines.AddRange(assignments);
        await context.SaveChangesAsync();
    }
}
