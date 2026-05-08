namespace GymAdmin.Application.DTOs.Users;

public record UserDto(int Id, string Nombre, string Email, string Rol, bool Activo, DateTime FechaCreacion);

public record ChangeRoleRequest(string Rol);
