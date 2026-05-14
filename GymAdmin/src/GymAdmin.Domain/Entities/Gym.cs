namespace GymAdmin.Domain.Entities;

public class Gym
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string DuenoNombreApellido { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string ColorPrincipalHex { get; set; } = "#2563EB";
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    public ICollection<Routine> Routines { get; set; } = new List<Routine>();
    public ICollection<StudentRoutine> StudentRoutines { get; set; } = new List<StudentRoutine>();
}
