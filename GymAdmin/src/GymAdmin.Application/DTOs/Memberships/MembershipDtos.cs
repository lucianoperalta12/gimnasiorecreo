namespace GymAdmin.Application.DTOs.Memberships;

public record MembershipDto(
    int Id,
    int GymId,
    string? GymNombre,
    int AlumnoId,
    string AlumnoNombre,
    string AlumnoApellido,
    string AlumnoEmail,
    int PlanId,
    string PlanNombre,
    decimal PlanPrecio,
    int PlanDuracionDias,
    DateTime FechaInicio,
    DateTime FechaVencimiento,
    string Estado,
    string? Notas,
    DateTime FechaCreacion,
    int DiasRestantes,
    string EstadoAcceso,
    string? Moneda = null
);

public record MembershipListDto(
    int Id,
    int GymId,
    int AlumnoId,
    string AlumnoNombreCompleto,
    string AlumnoDni,
    string PlanNombre,
    DateTime FechaInicio,
    DateTime FechaVencimiento,
    string Estado,
    string EstadoAcceso,
    int DiasRestantes,
    int IngresosUtilizados,
    bool PaseLibre = true,
    int? DiasPorSemana = null,
    bool PuedeRenovar = false,
    string? AlumnoTelefono = null
);

public record CreateMembershipRequest(
    int AlumnoId,
    int PlanId,
    DateTime FechaInicio,
    string? Notas
);

public record RenewMembershipRequest(
    int PlanId,
    DateTime? FechaInicio,
    string? Notas
);

public record CancelMembershipRequest(string? Motivo);

public record DashboardMembershipItemDto(
    int Id,
    int AlumnoId,
    string AlumnoNombreCompleto,
    string? AlumnoDni,
    string? AlumnoTelefono,
    string PlanNombre,
    DateTime FechaVencimiento
);

public record DashboardMembershipSummaryDto(
    IReadOnlyList<DashboardMembershipItemDto> PorVencer,
    IReadOnlyList<DashboardMembershipItemDto> Vencidas
);

public record StudentAccessDto(
    int AlumnoId,
    string AlumnoNombre,
    string AlumnoApellido,
    string AlumnoEmail,
    int GymId,
    string EstadoAcceso,
    int? MembresiaActivaId,
    string? PlanNombre,
    DateTime? FechaVencimiento,
    int? DiasRestantes,
    string? Moneda = null
);
