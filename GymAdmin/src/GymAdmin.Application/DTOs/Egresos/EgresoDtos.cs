namespace GymAdmin.Application.DTOs.Egresos;

public record EgresoDto(
    int Id,
    int GymId,
    string Descripcion,
    string Categoria,
    decimal Monto,
    DateTime Fecha,
    string? Observaciones,
    DateTime FechaCreacion
);

public record EgresoListDto(
    int Id,
    int GymId,
    string Descripcion,
    string Categoria,
    decimal Monto,
    DateTime Fecha,
    string? Observaciones
);

public record CreateEgresoRequest(
    string Descripcion,
    string Categoria,
    decimal Monto,
    DateTime Fecha,
    string? Observaciones = null
);

public record UpdateEgresoRequest(
    string Descripcion,
    string Categoria,
    decimal Monto,
    DateTime Fecha,
    string? Observaciones
);
