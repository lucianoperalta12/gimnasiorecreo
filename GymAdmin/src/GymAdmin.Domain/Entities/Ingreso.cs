namespace GymAdmin.Domain.Entities;

public class Ingreso
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public int AlumnoId { get; set; }
    public int TerminalId { get; set; }
    public int MembershipId { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.Now;

    public Gym Gym { get; set; } = null!;
    public User Alumno { get; set; } = null!;
    public User Terminal { get; set; } = null!;
    public Membership Membership { get; set; } = null!;
}
