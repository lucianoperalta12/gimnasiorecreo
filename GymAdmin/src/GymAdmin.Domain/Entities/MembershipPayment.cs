using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class MembershipPayment
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public int MembresiaId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? MetodoPago { get; set; }
    public PaymentStatus Estado { get; set; } = PaymentStatus.Completado;
    public string? Referencia { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Gym Gym { get; set; } = null!;
    public Membership Membresia { get; set; } = null!;
}
