using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;

namespace GymAdmin.Application.Helpers;

/// <summary>
/// Deriva el estado de acceso del alumno a partir de su membresía vigente.
/// La lógica no reside en la entidad User.
/// </summary>
public static class AccessStatusHelper
{
    public static AccessStatus DeriveFromMembership(
        Membership? membresia,
        bool hasPendingPayments = false,
        DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;

        if (membresia is null)
            return AccessStatus.SinMembresia;

        if (membresia.Estado == MembershipStatus.Suspendida)
            return AccessStatus.Suspendido;

        if (membresia.Estado == MembershipStatus.Cancelada)
            return AccessStatus.SinMembresia;

        if (membresia.Estado == MembershipStatus.Vencida || membresia.FechaVencimiento < now)
            return AccessStatus.Vencido;

        if (membresia.Estado == MembershipStatus.Activa)
        {
            if (hasPendingPayments)
                return AccessStatus.Moroso;
            return AccessStatus.Activo;
        }

        return AccessStatus.SinMembresia;
    }

    public static int DiasRestantes(Membership membresia, DateTime? utcNow = null)
    {
        var now = (utcNow ?? DateTime.UtcNow).Date;
        var vencimiento = membresia.FechaVencimiento.Date;
        return Math.Max(0, (vencimiento - now).Days);
    }

    public static string ToDisplayString(AccessStatus status) => status switch
    {
        AccessStatus.Activo => "Activo",
        AccessStatus.Vencido => "Vencido",
        AccessStatus.Moroso => "Moroso",
        AccessStatus.Suspendido => "Suspendido",
        AccessStatus.SinMembresia => "Sin membresía",
        _ => status.ToString()
    };
}
