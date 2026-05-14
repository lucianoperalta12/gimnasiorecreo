namespace GymAdmin.Application.DTOs.Gyms;

public record GymDto(int Id, string Nombre, string DuenoNombreApellido, string? LogoUrl, string ColorPrincipalHex, bool Activo);
public record CreateGymRequest(string Nombre, string DuenoNombreApellido, string? LogoUrl, string ColorPrincipalHex);
public record UpdateGymRequest(string Nombre, string DuenoNombreApellido, string? LogoUrl, string ColorPrincipalHex);
