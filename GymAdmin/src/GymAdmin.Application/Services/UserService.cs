using GymAdmin.Application.DTOs.Users;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .Select(u => new UserDto(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.Activo, u.FechaCreacion))
            .ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null
            ? null
            : new UserDto(user.Id, user.Nombre, user.Email, user.Rol.ToString(), user.Activo, user.FechaCreacion);
    }

    public async Task<List<UserDto>> GetStudentsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Rol == UserRole.Alumno)
            .OrderBy(u => u.Nombre)
            .Select(u => new UserDto(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.Activo, u.FechaCreacion))
            .ToListAsync();
    }

    public async Task<UserDto> ChangeRoleAsync(int userId, ChangeRoleRequest request)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (!Enum.TryParse<UserRole>(request.Rol, true, out var newRole))
            throw new ArgumentException($"Rol inválido: {request.Rol}. Valores válidos: Alumno, Profesor, Superusuario.");

        user.Rol = newRole;
        await _context.SaveChangesAsync();

        return new UserDto(user.Id, user.Nombre, user.Email, user.Rol.ToString(), user.Activo, user.FechaCreacion);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> ToggleStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.Activo = !user.Activo;
        await _context.SaveChangesAsync();

        return new UserDto(user.Id, user.Nombre, user.Email, user.Rol.ToString(), user.Activo, user.FechaCreacion);
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
