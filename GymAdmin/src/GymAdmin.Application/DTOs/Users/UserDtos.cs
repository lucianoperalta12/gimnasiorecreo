namespace GymAdmin.Application.DTOs.Users;

public record UserDto(
    int Id,
    string Nombre,
    string Apellido,
    string Email,
    string Dni,
    string Rol,
    bool Activo,
    bool DebeCambiarPassword,
    int GymId,
    string? GymNombre,
    string? GymColorPrincipalHex,
    string? GymLogoUrl,
    bool GymVeRutinas,
    DateTime FechaCreacion
);

public record CreateUserRequest(string Nombre, string Apellido, string Email, string Dni, string Rol, int? GymId);

public record UpdateUserRequest(string Nombre, string Apellido);

public record ChangeRoleRequest(string Rol);
public record ChangePasswordRequest(string Password);

