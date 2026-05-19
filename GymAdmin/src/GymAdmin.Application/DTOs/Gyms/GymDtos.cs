namespace GymAdmin.Application.DTOs.Gyms;

public record GymDto(int Id, string Nombre, string DuenoNombreApellido, string? LogoUrl, string ColorPrincipalHex, bool Activo, string Moneda, bool VeRutinas);
public record CreateGymRequest(string Nombre, string DuenoNombreApellido, string? LogoUrl, string ColorPrincipalHex, string Moneda, bool VeRutinas);
public record UpdateGymRequest(string Nombre, string DuenoNombreApellido, string? LogoUrl, string ColorPrincipalHex, string Moneda, bool VeRutinas);
