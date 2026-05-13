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

        // 1. Limpieza de datos desactivada para permitir persistencia
        /*
        if (await context.Users.AnyAsync(u => u.Email != "admin") || await context.Routines.AnyAsync())
        {
            context.StudentRoutines.RemoveRange(context.StudentRoutines);
            context.RoutineExercises.RemoveRange(context.RoutineExercises);
            context.Routines.RemoveRange(context.Routines);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }
        */

        // 2. Asegurar admin solicitado
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Nombre = "admin",
                Email = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Rol = UserRole.Superusuario,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // 3. Mantener ejercicios
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
    }
}
