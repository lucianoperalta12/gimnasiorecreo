namespace GymAdmin.Application.DTOs.Ingresos;

public record RegistrarIngresoRequest(string Dni);

public record RegistrarIngresoResponse(
    int IngresoId,
    int AlumnoId,
    string AlumnoNombreCompleto,
    string Dni,
    string Gimnasio,
    DateTime FechaHora,
    string TerminalNombreCompleto,
    string TipoMembresia,
    bool PaseLibre,
    int IngresosUtilizados,
    DateTime FechaVencimiento
);

public record IngresoListItemDto(
    int Id,
    string Alumno,
    string Dni,
    string Gimnasio,
    DateTime FechaHora,
    string Terminal,
    string TipoMembresia
);
