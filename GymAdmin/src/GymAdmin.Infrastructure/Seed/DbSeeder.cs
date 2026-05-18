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

        var gym = await context.Gyms.FirstOrDefaultAsync() ?? new Gym
        {
            Nombre = "Gimnasio Central",
            DuenoNombreApellido = "Administrador General",
            ColorPrincipalHex = "#ff6600",
            Activo = true
        };
        if (gym.Id == 0)
        {
            context.Gyms.Add(gym);
            await context.SaveChangesAsync();
        }

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                Nombre = "admin",
                Apellido = "admin",
                Email = "admin",
                Dni = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Rol = UserRole.Superusuario,
                GymId = gym.Id,
                Activo = true,
                DebeCambiarPassword = false
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        if (!await context.Exercises.AnyAsync())
        {
            context.Exercises.AddRange(
                new Exercise { GymId = gym.Id, Nombre = "Press de banca", GrupoMuscular = "Pecho" },
                new Exercise { GymId = gym.Id, Nombre = "Sentadilla", GrupoMuscular = "Piernas" },
                new Exercise { GymId = gym.Id, Nombre = "Peso muerto", GrupoMuscular = "Espalda" }
            );
            await context.SaveChangesAsync();
        }
    }
}
