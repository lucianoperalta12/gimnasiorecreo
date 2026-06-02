using GymAdmin.Domain.Enums;

namespace GymAdmin.Domain.Entities;

public class Membership
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public int AlumnoId { get; set; }
    public int PlanId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public MembershipStatus Estado { get; set; } = MembershipStatus.Activa;
    public int IngresosUtilizados { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Gym Gym { get; set; } = null!;
    public User Alumno { get; set; } = null!;
    public MembershipPlan Plan { get; set; } = null!;
    public ICollection<MembershipPayment> Pagos { get; set; } = new List<MembershipPayment>();
    public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}
