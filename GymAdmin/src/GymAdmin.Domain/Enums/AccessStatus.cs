namespace GymAdmin.Domain.Enums;

/// <summary>
/// Estado de acceso derivado de la membresía vigente del alumno.
/// No se persiste en User; se calcula en capa de aplicación.
/// </summary>
public enum AccessStatus
{
    Activo = 0,
    Vencido = 1,
    Moroso = 2,
    Suspendido = 3,
    SinMembresia = 4
}
