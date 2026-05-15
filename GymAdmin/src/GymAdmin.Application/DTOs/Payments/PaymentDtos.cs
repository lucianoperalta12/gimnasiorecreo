namespace GymAdmin.Application.DTOs.Payments;

public record PaymentDto(
    int Id,
    int GymId,
    string? GymNombre,
    int MembresiaId,
    int AlumnoId,
    string AlumnoNombreCompleto,
    decimal Monto,
    DateTime FechaPago,
    string? MetodoPago,
    string Estado,
    string? Referencia,
    string? Notas,
    DateTime FechaCreacion
);

public record PaymentListDto(
    int Id,
    int MembresiaId,
    string AlumnoNombreCompleto,
    decimal Monto,
    DateTime FechaPago,
    string Estado,
    string? MetodoPago
);

public record CreatePaymentRequest(
    int MembresiaId,
    decimal Monto,
    DateTime FechaPago,
    string? MetodoPago,
    string Estado = "Completado",
    string? Referencia = null,
    string? Notas = null
);

public record UpdatePaymentRequest(
    decimal Monto,
    DateTime FechaPago,
    string? MetodoPago,
    string Estado,
    string? Referencia,
    string? Notas
);
