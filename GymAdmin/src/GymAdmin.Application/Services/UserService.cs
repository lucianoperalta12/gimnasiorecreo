using GymAdmin.Application.DTOs.Users;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context) { _context = context; }

    public async Task<List<UserDto>> GetAllUsersAsync(int requesterId)
    {
        var requester = await GetRequester(requesterId);
        var query = _context.Users.AsNoTracking().Include(u => u.Gym).AsQueryable();
        if (requester.Rol == UserRole.Administrativo) query = query.Where(u => u.GymId == requester.GymId && (u.Rol == UserRole.Alumno || u.Rol == UserRole.Profesor));
        
        var data = await query.OrderBy(u => u.Nombre).ThenBy(u => u.Apellido)
            .Select(u => new {
                u.Id, u.Nombre, u.Apellido, u.Email, u.Dni, Rol = u.Rol, u.Activo, u.DebeCambiarPassword, u.GymId, 
                GymNombre = u.Gym != null ? u.Gym.Nombre : null, 
                GymColor = u.Gym != null ? u.Gym.ColorPrincipalHex : null, 
                GymLogo = u.Gym != null ? u.Gym.LogoUrl : null, u.FechaCreacion
            }).ToListAsync();

        return data.Select(u => new UserDto(u.Id, u.Nombre, u.Apellido, u.Email, u.Dni, u.Rol.ToString(), u.Activo, u.DebeCambiarPassword, u.GymId, u.GymNombre, u.GymColor, u.GymLogo, u.FechaCreacion)).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequester(requesterId);
        var query = _context.Users.AsNoTracking().Include(u => u.Gym).Where(u => u.Id == id);
        if (requester.Rol == UserRole.Administrativo) query = query.Where(u => u.GymId == requester.GymId);
        
        var u = await query.Select(u => new {
                u.Id, u.Nombre, u.Apellido, u.Email, u.Dni, Rol = u.Rol, u.Activo, u.DebeCambiarPassword, u.GymId, 
                GymNombre = u.Gym != null ? u.Gym.Nombre : null, 
                GymColor = u.Gym != null ? u.Gym.ColorPrincipalHex : null, 
                GymLogo = u.Gym != null ? u.Gym.LogoUrl : null, u.FechaCreacion
            }).FirstOrDefaultAsync();

        if (u == null) return null;
        return new UserDto(u.Id, u.Nombre, u.Apellido, u.Email, u.Dni, u.Rol.ToString(), u.Activo, u.DebeCambiarPassword, u.GymId, u.GymNombre, u.GymColor, u.GymLogo, u.FechaCreacion);
    }

    public async Task<List<UserDto>> GetStudentsAsync(int requesterId)
    {
        var requester = await GetRequester(requesterId);
        var query = _context.Users.AsNoTracking().Include(u => u.Gym).Where(u => u.Rol == UserRole.Alumno);
        if (requester.Rol != UserRole.Superusuario) query = query.Where(u => u.GymId == requester.GymId);
        
        var data = await query.OrderBy(u => u.Nombre).ThenBy(u => u.Apellido)
            .Select(u => new {
                u.Id, u.Nombre, u.Apellido, u.Email, u.Dni, Rol = u.Rol, u.Activo, u.DebeCambiarPassword, u.GymId, 
                GymNombre = u.Gym != null ? u.Gym.Nombre : null, 
                GymColor = u.Gym != null ? u.Gym.ColorPrincipalHex : null, 
                GymLogo = u.Gym != null ? u.Gym.LogoUrl : null, u.FechaCreacion
            }).ToListAsync();

        return data.Select(u => new UserDto(u.Id, u.Nombre, u.Apellido, u.Email, u.Dni, u.Rol.ToString(), u.Activo, u.DebeCambiarPassword, u.GymId, u.GymNombre, u.GymColor, u.GymLogo, u.FechaCreacion)).ToList();
    }

    public async Task<UserDto> CreateUserAsync(int requesterId, CreateUserRequest request)
    {
        var requester = await GetRequester(requesterId);
        if (!Enum.TryParse<UserRole>(request.Rol, true, out var newRole)) throw new ArgumentException("Rol inválido.");
        if (requester.Rol == UserRole.Superusuario && newRole == UserRole.Superusuario) throw new ArgumentException("No se puede crear Superusuario desde panel.");
        if (requester.Rol == UserRole.Administrativo && (newRole == UserRole.Administrativo || newRole == UserRole.Superusuario)) throw new UnauthorizedAccessException("No autorizado.");
        if (await _context.Users.AnyAsync(u => u.Email == request.Email || u.Dni == request.Dni)) throw new InvalidOperationException("Email o DNI ya existe.");

        var gymId = requester.Rol == UserRole.Superusuario ? request.GymId ?? 0 : requester.GymId;
        if (gymId <= 0) throw new ArgumentException("Debe indicar gimnasio.");

        var user = new User
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = request.Email.Trim(),
            Dni = request.Dni.Trim(),
            Rol = newRole,
            GymId = gymId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Dni.Trim()),
            DebeCambiarPassword = true,
            Activo = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return (await GetUserByIdAsync(requesterId, user.Id))!;
    }

    public async Task<UserDto> ChangeRoleAsync(int requesterId, int userId, ChangeRoleRequest request)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users.Include(u => u.Gym).FirstOrDefaultAsync(u => u.Id == userId) 
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (!Enum.TryParse<UserRole>(request.Rol, true, out var newRole)) throw new ArgumentException("Rol inválido.");

        if (requester.Rol == UserRole.Superusuario) { /* OK */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (user.GymId != requester.GymId) throw new UnauthorizedAccessException("No autorizado para este gimnasio.");
            if (newRole == UserRole.Superusuario || newRole == UserRole.Administrativo)
                throw new UnauthorizedAccessException("No autorizado para asignar este rol.");
            
            if (user.Rol != UserRole.Alumno && user.Rol != UserRole.Profesor)
                throw new UnauthorizedAccessException("No autorizado para cambiar el rol de este usuario.");
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        user.Rol = newRole;

        // Si el nuevo rol es Profesor, cancelamos cualquier membresía activa que pudiera tener
        if (newRole == UserRole.Profesor)
        {
            var activeMemberships = await _context.Memberships
                .Where(m => m.AlumnoId == userId && m.Estado == MembershipStatus.Activa)
                .ToListAsync();
            
            foreach (var m in activeMemberships)
            {
                m.Estado = MembershipStatus.Vencida;
                m.Notas = (string.IsNullOrWhiteSpace(m.Notas) ? "" : m.Notas + " | ") + "Cancelada automáticamente por cambio de rol a Profesor.";
            }
        }

        await _context.SaveChangesAsync();
        return (await GetUserByIdAsync(requesterId, user.Id))!;
    }

    public async Task ChangePasswordAsync(int requesterId, int userId, ChangePasswordRequest request)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");
        
        if (requester.Rol == UserRole.Superusuario) { /* Allow all */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (user.GymId != requester.GymId || (user.Rol != UserRole.Alumno && user.Rol != UserRole.Profesor))
                throw new UnauthorizedAccessException("No autorizado para cambiar esta contraseña.");
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.DebeCambiarPassword = false;
        await _context.SaveChangesAsync();
    }

    public async Task ChangeMyInitialPasswordAsync(int requesterId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(requesterId) ?? throw new KeyNotFoundException("Usuario no encontrado.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.DebeCambiarPassword = false;
        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> ToggleStatusAsync(int requesterId, int userId)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users.Include(u => u.Gym).FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");
        if (requester.Rol == UserRole.Administrativo && (user.GymId != requester.GymId || (user.Rol != UserRole.Alumno && user.Rol != UserRole.Profesor)))
            throw new UnauthorizedAccessException("No autorizado.");
        user.Activo = !user.Activo;
        await _context.SaveChangesAsync();
        return (await GetUserByIdAsync(requesterId, user.Id))!;
    }

    public async Task DeleteUserAsync(int requesterId, int userId)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (userId == requesterId) throw new InvalidOperationException("No puedes eliminar tu propio usuario.");
        if (user.Email == "admin" || user.Nombre.ToLower() == "admin") throw new InvalidOperationException("No se puede eliminar el administrador del sistema.");

        if (requester.Rol == UserRole.Superusuario) { /* OK */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (user.GymId != requester.GymId) throw new UnauthorizedAccessException("No autorizado para este gimnasio.");
            if (user.Rol != UserRole.Alumno && user.Rol != UserRole.Profesor)
                throw new UnauthorizedAccessException("No autorizado para eliminar este tipo de usuario.");
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        try 
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("No se pudo eliminar el usuario porque tiene registros vinculados (pagos, rutinas, etc.). Pruebe desactivándolo en lugar de borrarlo.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"No se pudo eliminar el usuario: {ex.Message}");
        }
    }

    private async Task<User> GetRequester(int requesterId) => await _context.Users.FindAsync(requesterId) ?? throw new UnauthorizedAccessException("Usuario inválido.");
}
