namespace GymAdmin.Domain.Entities;

public class Gym
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string DuenoNombreApellido { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string ColorPrincipalHex { get; set; } = "#ff6600";
    public bool Activo { get; set; } = true;
    public string Moneda { get; set; } = "ARS"; // ARS o USD
    public bool VeRutinas { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<GymUser> GymUsers { get; set; } = new List<GymUser>();
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<Routine> Routines { get; set; } = new List<Routine>();
    public ICollection<StudentRoutine> StudentRoutines { get; set; } = new List<StudentRoutine>();
    public ICollection<MembershipPlan> MembershipPlans { get; set; } = new List<MembershipPlan>();
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<MembershipPayment> MembershipPayments { get; set; } = new List<MembershipPayment>();
    public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
}
