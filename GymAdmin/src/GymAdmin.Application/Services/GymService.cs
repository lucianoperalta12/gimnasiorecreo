using GymAdmin.Application.DTOs.Gyms;
using GymAdmin.Domain.Entities;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class GymService : IGymService
{
    private readonly AppDbContext _context;

    public GymService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GymDto>> GetAllAsync() =>
        await _context.Gyms.AsNoTracking().OrderBy(g => g.Nombre)
            .Select(g => new GymDto(g.Id, g.Nombre, g.DuenoNombreApellido, g.LogoUrl, g.ColorPrincipalHex, g.Activo, g.Moneda))
            .ToListAsync();

    public async Task<GymDto?> GetByIdAsync(int id) =>
        await _context.Gyms.AsNoTracking().Where(g => g.Id == id)
            .Select(g => new GymDto(g.Id, g.Nombre, g.DuenoNombreApellido, g.LogoUrl, g.ColorPrincipalHex, g.Activo, g.Moneda))
            .FirstOrDefaultAsync();

    public async Task<GymDto> CreateAsync(CreateGymRequest request)
    {
        Validate(request.Nombre, request.DuenoNombreApellido, request.ColorPrincipalHex);
        var gym = new Gym
        {
            Nombre = request.Nombre.Trim(),
            DuenoNombreApellido = request.DuenoNombreApellido.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            ColorPrincipalHex = request.ColorPrincipalHex.Trim(),
            Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? "ARS" : request.Moneda.Trim(),
            Activo = true
        };

        _context.Gyms.Add(gym);
        await _context.SaveChangesAsync();
        return new GymDto(gym.Id, gym.Nombre, gym.DuenoNombreApellido, gym.LogoUrl, gym.ColorPrincipalHex, gym.Activo, gym.Moneda);
    }

    public async Task<GymDto> UpdateAsync(int id, UpdateGymRequest request)
    {
        Validate(request.Nombre, request.DuenoNombreApellido, request.ColorPrincipalHex);
        var gym = await _context.Gyms.FindAsync(id) ?? throw new KeyNotFoundException("Gimnasio no encontrado.");
        gym.Nombre = request.Nombre.Trim();
        gym.DuenoNombreApellido = request.DuenoNombreApellido.Trim();
        gym.LogoUrl = request.LogoUrl?.Trim();
        gym.ColorPrincipalHex = request.ColorPrincipalHex.Trim();
        gym.Moneda = string.IsNullOrWhiteSpace(request.Moneda) ? "ARS" : request.Moneda.Trim();
        await _context.SaveChangesAsync();
        return new GymDto(gym.Id, gym.Nombre, gym.DuenoNombreApellido, gym.LogoUrl, gym.ColorPrincipalHex, gym.Activo, gym.Moneda);
    }

    public async Task<GymDto> ToggleStatusAsync(int id)
    {
        var gym = await _context.Gyms.FindAsync(id) ?? throw new KeyNotFoundException("Gimnasio no encontrado.");
        gym.Activo = !gym.Activo;
        await _context.SaveChangesAsync();
        return new GymDto(gym.Id, gym.Nombre, gym.DuenoNombreApellido, gym.LogoUrl, gym.ColorPrincipalHex, gym.Activo, gym.Moneda);
    }

    private static void Validate(string nombre, string dueno, string color)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre del gimnasio es obligatorio.");
        if (string.IsNullOrWhiteSpace(dueno)) throw new ArgumentException("El nombre del dueño es obligatorio.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(color.Trim(), "^#[0-9A-Fa-f]{6}$"))
            throw new ArgumentException("El color principal debe tener formato hexadecimal, por ejemplo #2563EB.");
    }
}
