using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Users;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public UserService(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResult<UserDto>> GetAllUsersAsync(int requesterId, string? search = null, string? rol = null, int? gymId = null, int? page = null, int? pageSize = null)
    {
        var requester = await GetRequester(requesterId);
        var defaultGymId = requester.GymId;

        var query = _context.GymUsers.AsNoTracking()
            .Include(gu => gu.User)
            .Include(gu => gu.Gym)
            .AsQueryable();

        if (requester.Rol == UserRole.Superusuario)
        {
            // Una fila por cada asociacion usuario-gimnasio (mismo DNI puede repetirse).
        }
        else if (requester.Rol == UserRole.Administrativo)
        {
            query = query.Where(gu =>
                gu.GymId == defaultGymId &&
                (gu.Rol == UserRole.Alumno || gu.Rol == UserRole.Profesor));
        }
        else
        {
            throw new UnauthorizedAccessException("No autorizado.");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(gu =>
                gu.User.Nombre.ToLower().Contains(cleanSearch) ||
                gu.User.Apellido.ToLower().Contains(cleanSearch) ||
                gu.User.Email.ToLower().Contains(cleanSearch) ||
                gu.User.Dni.ToLower().Contains(cleanSearch));
        }

        if (!string.IsNullOrWhiteSpace(rol))
        {
            if (Enum.TryParse<UserRole>(rol, true, out var targetRole))
            {
                query = query.Where(gu => gu.Rol == targetRole);
            }
        }

        if (gymId.HasValue)
        {
            if (requester.Rol == UserRole.Superusuario)
            {
                query = query.Where(gu => gu.GymId == gymId.Value);
            }
        }

        var totalCount = await query.CountAsync();
        var activeCount = await query.CountAsync(gu => gu.Activo && gu.User.Activo);
        var inactiveCount = totalCount - activeCount;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Headers["X-Active-Count"] = activeCount.ToString();
            httpContext.Response.Headers["X-Inactive-Count"] = inactiveCount.ToString();
        }

        var pagedQuery = ApplyPagination(
            query.OrderBy(gu => gu.User.Nombre)
                .ThenBy(gu => gu.User.Apellido)
                .ThenBy(gu => gu.Gym.Nombre),
            page,
            pageSize);

        var associations = await pagedQuery.ToListAsync();

        return new PagedResult<UserDto>(
            associations.Select(gu => MapToUserDto(gu.User, gu)).ToList(),
            totalCount,
            page,
            NormalizePageSize(pageSize));
    }

    public async Task<UserDto?> GetUserByIdAsync(int requesterId, int id, int? gymId = null)
    {
        var requester = await GetRequester(requesterId);
        var targetGymId = ResolveTargetGymId(requester, gymId);

        var query = _context.Users.AsNoTracking()
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .Where(u => u.Id == id);

        if (requester.Rol != UserRole.Superusuario)
        {
            query = query.Where(u => u.GymUsers.Any(gu => gu.GymId == targetGymId));
        }

        var u = await query.FirstOrDefaultAsync();
        if (u == null) return null;

        var gymUser = u.GymUsers.FirstOrDefault(gu => gu.GymId == targetGymId);
        if (gymUser == null) return null;

        if (requester.Rol == UserRole.Administrativo)
        {
            if (gymUser.GymId != requester.GymId || (gymUser.Rol != UserRole.Alumno && gymUser.Rol != UserRole.Profesor))
                return null;
        }

        return MapToUserDto(u, gymUser);
    }

    public async Task<PagedResult<UserDto>> GetStudentsAsync(int requesterId, int? page = null, int? pageSize = null)
    {
        var requester = await GetRequester(requesterId);
        var gymId = requester.GymId;

        var query = _context.Users.AsNoTracking()
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .AsQueryable();

        if (requester.Rol != UserRole.Superusuario)
        {
            query = query.Where(u => u.GymUsers.Any(gu => gu.GymId == gymId && gu.Rol == UserRole.Alumno));
        }
        else
        {
            query = query.Where(u => u.GymUsers.Any(gu => gu.Rol == UserRole.Alumno));
        }

        var totalCount = await query.CountAsync();
        var pagedQuery = ApplyPagination(query.OrderBy(u => u.Nombre).ThenBy(u => u.Apellido), page, pageSize);
        var data = await pagedQuery.ToListAsync();

        var items = data.Select(u => {
            var gymUser = u.GymUsers.FirstOrDefault(gu => gu.GymId == gymId && gu.Rol == UserRole.Alumno)
                          ?? u.GymUsers.FirstOrDefault(gu => gu.Rol == UserRole.Alumno);
            return new UserDto(
                u.Id,
                u.Nombre,
                u.Apellido,
                u.Email,
                u.Dni,
                gymUser!.Rol.ToString(),
                gymUser.Activo && u.Activo,
                u.DebeCambiarPassword,
                gymUser.GymId,
                gymUser.Gym.Nombre,
                gymUser.Gym.ColorPrincipalHex,
                gymUser.Gym.LogoUrl,
                gymUser.Gym.VeRutinas,
                u.FechaCreacion,
                u.FechaNacimiento,
                u.Domicilio,
                u.Telefono,
                u.Observaciones
            );
        }).ToList();

        return new PagedResult<UserDto>(items, totalCount, page, NormalizePageSize(pageSize));
    }

    public async Task<UserDto> CreateUserAsync(int requesterId, CreateUserRequest request)
    {
        var requester = await GetRequester(requesterId);
        if (!Enum.TryParse<UserRole>(request.Rol, true, out var newRole)) throw new ArgumentException("Rol inválido.");
        if (requester.Rol == UserRole.Superusuario && newRole == UserRole.Superusuario) throw new ArgumentException("No se puede crear Superusuario desde panel.");
        if (requester.Rol != UserRole.Superusuario && newRole == UserRole.Terminal) throw new UnauthorizedAccessException("No autorizado.");
        if (requester.Rol == UserRole.Administrativo && (newRole == UserRole.Administrativo || newRole == UserRole.Superusuario || newRole == UserRole.Terminal)) throw new UnauthorizedAccessException("No autorizado.");

        var gymId = requester.Rol == UserRole.Superusuario
            ? (request.GymId ?? requester.GymId)
            : requester.GymId;
        if (gymId <= 0) throw new ArgumentException("Debe indicar gimnasio.");

        var existingUser = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Email == request.Email || u.Dni == request.Dni);

        if (existingUser != null)
        {
            if (existingUser.GymUsers.Any(gu => gu.GymId == gymId))
            {
                throw new InvalidOperationException("Email o DNI ya existe en este gimnasio.");
            }

            var association = new GymUser
            {
                GymId = gymId,
                UserId = existingUser.Id,
                Rol = newRole,
                Activo = true,
                FechaAsociacion = DateTime.UtcNow
            };
            _context.GymUsers.Add(association);
            await _context.SaveChangesAsync();
            return (await GetUserByIdAsync(requesterId, existingUser.Id, gymId))!;
        }

        var user = new User
        {
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = request.Email.Trim(),
            Dni = request.Dni.Trim(),
            FechaNacimiento = request.FechaNacimiento.HasValue ? DateTime.SpecifyKind(request.FechaNacimiento.Value, DateTimeKind.Utc) : null,
            Domicilio = request.Domicilio,
            Telefono = request.Telefono,
            Observaciones = request.Observaciones,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Dni.Trim()),
            DebeCambiarPassword = true,
            Activo = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var newAssociation = new GymUser
        {
            GymId = gymId,
            UserId = user.Id,
            Rol = newRole,
            Activo = true,
            FechaAsociacion = DateTime.UtcNow
        };
        _context.GymUsers.Add(newAssociation);
        await _context.SaveChangesAsync();

        return (await GetUserByIdAsync(requesterId, user.Id, gymId))!;
    }

    public async Task<UserDto> UpdateUserAsync(int requesterId, int userId, UpdateUserRequest request)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == userId) 
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var gymId = requester.GymId;
        var gymUser = user.GymUsers.FirstOrDefault(gu => gu.GymId == gymId) ?? user.GymUsers.FirstOrDefault();
        if (gymUser == null) throw new UnauthorizedAccessException("El usuario no tiene asociaciones en el gimnasio.");

        if (requester.Rol == UserRole.Superusuario) { /* OK */ }
        else if (userId == requesterId) { /* Ownership OK */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (gymUser.GymId != gymId || (gymUser.Rol != UserRole.Alumno && gymUser.Rol != UserRole.Profesor))
                throw new UnauthorizedAccessException("No autorizado para modificar este usuario.");
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Apellido))
            throw new ArgumentException("Nombre y Apellido son requeridos.");

        if (request.FechaNacimiento.HasValue && request.FechaNacimiento.Value > DateTime.UtcNow)
            throw new ArgumentException("La fecha de nacimiento no puede ser una fecha futura.");

        user.Nombre = request.Nombre.Trim();
        user.Apellido = request.Apellido.Trim();
        user.FechaNacimiento = request.FechaNacimiento.HasValue ? DateTime.SpecifyKind(request.FechaNacimiento.Value, DateTimeKind.Utc) : null;
        user.Domicilio = request.Domicilio;
        user.Telefono = request.Telefono;
        user.Observaciones = request.Observaciones;

        await _context.SaveChangesAsync();
        return (await GetUserByIdAsync(requesterId, user.Id))!;
    }

    public async Task<UserDto> ChangeRoleAsync(int requesterId, int userId, ChangeRoleRequest request)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == userId) 
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (!Enum.TryParse<UserRole>(request.Rol, true, out var newRole)) throw new ArgumentException("Rol inválido.");

        var gymId = ResolveTargetGymId(requester, request.GymId);
        var gymUser = user.GymUsers.FirstOrDefault(gu => gu.GymId == gymId);
        if (gymUser == null) throw new UnauthorizedAccessException("El usuario no pertenece a este gimnasio.");

        if (requester.Rol == UserRole.Superusuario) { /* OK */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (gymUser.GymId != gymId) throw new UnauthorizedAccessException("No autorizado para este gimnasio.");
            if (newRole == UserRole.Superusuario || newRole == UserRole.Administrativo || newRole == UserRole.Terminal)
                throw new UnauthorizedAccessException("No autorizado para asignar este rol.");
            
            if (gymUser.Rol != UserRole.Alumno && gymUser.Rol != UserRole.Profesor)
                throw new UnauthorizedAccessException("No autorizado para cambiar el rol de este usuario.");
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        gymUser.Rol = newRole;

        if (newRole == UserRole.Profesor)
        {
            var activeMemberships = await _context.Memberships
                .Where(m => m.AlumnoId == userId && m.GymId == gymId && m.Estado == MembershipStatus.Activa)
                .ToListAsync();
            
            foreach (var m in activeMemberships)
            {
                m.Estado = MembershipStatus.Vencida;
                m.Notas = (string.IsNullOrWhiteSpace(m.Notas) ? "" : m.Notas + " | ") + "Cancelada automáticamente por cambio de rol a Profesor.";
            }
        }

        await _context.SaveChangesAsync();
        return (await GetUserByIdAsync(requesterId, user.Id, gymId))!;
    }

    public async Task ChangePasswordAsync(int requesterId, int userId, ChangePasswordRequest request)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");
        
        var gymId = requester.GymId;
        var gymUser = user.GymUsers.FirstOrDefault(gu => gu.GymId == gymId) ?? user.GymUsers.FirstOrDefault();
        if (gymUser == null) throw new UnauthorizedAccessException("El usuario no tiene asociaciones.");

        if (requester.Rol == UserRole.Superusuario) { /* Allow all */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (gymUser.GymId != gymId || (gymUser.Rol != UserRole.Alumno && gymUser.Rol != UserRole.Profesor))
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

    public async Task<UserDto> ToggleStatusAsync(int requesterId, int userId, int? gymId = null)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var targetGymId = ResolveTargetGymId(requester, gymId);
        var gymUser = user.GymUsers.FirstOrDefault(gu => gu.GymId == targetGymId);
        if (gymUser == null) throw new UnauthorizedAccessException("El usuario no pertenece a este gimnasio.");

        if (requester.Rol == UserRole.Superusuario)
        {
            gymUser.Activo = !gymUser.Activo;
        }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (gymUser.GymId != targetGymId || (gymUser.Rol != UserRole.Alumno && gymUser.Rol != UserRole.Profesor))
                throw new UnauthorizedAccessException("No autorizado.");
            gymUser.Activo = !gymUser.Activo;
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        await _context.SaveChangesAsync();
        return (await GetUserByIdAsync(requesterId, user.Id, targetGymId))!;
    }

    public async Task DeleteUserAsync(int requesterId, int userId, int? gymId = null)
    {
        var requester = await GetRequester(requesterId);
        var user = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("Usuario no encontrado.");

        if (userId == requesterId) throw new InvalidOperationException("No puedes eliminar tu propio usuario.");
        if (user.Email == "admin" || user.Nombre.ToLower() == "admin") throw new InvalidOperationException("No se puede eliminar el administrador del sistema.");

        var targetGymId = ResolveTargetGymId(requester, gymId);
        var gymUser = user.GymUsers.FirstOrDefault(gu => gu.GymId == targetGymId);

        if (requester.Rol == UserRole.Superusuario) { /* OK */ }
        else if (requester.Rol == UserRole.Administrativo)
        {
            if (gymUser == null || gymUser.GymId != targetGymId) throw new UnauthorizedAccessException("No autorizado para este gimnasio.");
            if (gymUser.Rol != UserRole.Alumno && gymUser.Rol != UserRole.Profesor)
                throw new UnauthorizedAccessException("No autorizado para eliminar este tipo de usuario.");
        }
        else throw new UnauthorizedAccessException("No autorizado.");

        try 
        {
            if (gymUser == null)
                throw new UnauthorizedAccessException("No autorizado para este gimnasio.");

            if (user.GymUsers.Count > 1)
            {
                _context.GymUsers.Remove(gymUser);
            }
            else
            {
                _context.Users.Remove(user);
            }
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

    private async Task<User> GetRequester(int requesterId)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == requesterId)
            ?? throw new UnauthorizedAccessException("Usuario inválido.");

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var gymIdClaim = httpContext.User.FindFirst("gymId")?.Value;
            if (int.TryParse(gymIdClaim, out var gymId))
            {
                user.GymId = gymId;
            }

            var roleClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (Enum.TryParse<UserRole>(roleClaim, true, out var role))
            {
                user.Rol = role;
            }
            else
            {
                var association = user.GymUsers.FirstOrDefault(gu => gu.GymId == user.GymId && gu.Activo)
                    ?? user.GymUsers.FirstOrDefault(gu => gu.Activo);
                if (association != null)
                {
                    user.Rol = association.Rol;
                    if (user.GymId == 0)
                    {
                        user.GymId = association.GymId;
                    }
                }
            }
        }

        return user;
    }

    private static int ResolveTargetGymId(User requester, int? requestedGymId)
    {
        if (requestedGymId is > 0)
        {
            if (requester.Rol != UserRole.Superusuario && requestedGymId.Value != requester.GymId)
                throw new UnauthorizedAccessException("No autorizado para este gimnasio.");

            return requestedGymId.Value;
        }

        if (requester.GymId <= 0)
            throw new UnauthorizedAccessException("Contexto de gimnasio no definido.");

        return requester.GymId;
    }

    private static UserDto MapToUserDto(User user, GymUser gymUser) => new(
        user.Id,
        user.Nombre,
        user.Apellido,
        user.Email,
        user.Dni,
        gymUser.Rol.ToString(),
        gymUser.Activo && user.Activo,
        user.DebeCambiarPassword,
        gymUser.GymId,
        gymUser.Gym.Nombre,
        gymUser.Gym.ColorPrincipalHex,
        gymUser.Gym.LogoUrl,
        gymUser.Gym.VeRutinas,
        user.FechaCreacion,
        user.FechaNacimiento,
        user.Domicilio,
        user.Telefono,
        user.Observaciones
    );

    private static IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int? page, int? pageSize)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);
        if (!normalizedPageSize.HasValue)
            return query;

        var normalizedPage = NormalizePage(page);
        return query.Skip((normalizedPage - 1) * normalizedPageSize.Value).Take(normalizedPageSize.Value);
    }

    private static int NormalizePage(int? page) => page.GetValueOrDefault(1) > 0 ? page.GetValueOrDefault(1) : 1;

    private static int? NormalizePageSize(int? pageSize)
    {
        if (!pageSize.HasValue || pageSize.Value <= 0)
            return null;

        return Math.Min(pageSize.Value, 200);
    }
}
