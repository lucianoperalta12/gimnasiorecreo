namespace GymAdmin.Application.DTOs.MembershipPlans;

public record MembershipPlanDto(
    int Id,
    int GymId,
    string? GymNombre,
    string Nombre,
    string? Descripcion,
    int DuracionDias,
    decimal Precio,
    bool Activo,
    DateTime FechaCreacion,
    string? Moneda = null
);

public record CreateMembershipPlanRequest(
    string Nombre,
    string? Descripcion,
    int DuracionDias,
    decimal Precio,
    bool Activo = true,
    int? GymId = null
);

public record UpdateMembershipPlanRequest(
    string Nombre,
    string? Descripcion,
    int DuracionDias,
    decimal Precio,
    bool Activo
);
