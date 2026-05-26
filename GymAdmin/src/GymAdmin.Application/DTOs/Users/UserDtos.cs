using System.ComponentModel.DataAnnotations;

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
    DateTime FechaCreacion,
    DateTime? FechaNacimiento,
    string? Domicilio,
    string? Telefono,
    string? Observaciones
);

public record CreateUserRequest(string Nombre, string Apellido, string Email, string Dni, string Rol, int? GymId, DateTime? FechaNacimiento, string? Domicilio, string? Telefono, string? Observaciones);

public record UpdateUserRequest(
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es requerido.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    string Apellido,

    DateTime? FechaNacimiento,

    [StringLength(200, ErrorMessage = "El domicilio no puede superar los 200 caracteres.")]
    string? Domicilio,

    [StringLength(50, ErrorMessage = "El teléfono no puede superar los 50 caracteres.")]
    string? Telefono,

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    string? Observaciones
);

public record ChangeRoleRequest(string Rol);
public record ChangePasswordRequest(string Password);

