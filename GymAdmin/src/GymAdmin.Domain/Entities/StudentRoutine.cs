namespace GymAdmin.Domain.Entities;

public class StudentRoutine
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public int AlumnoId { get; set; }
    public int RutinaId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
    public bool Activa { get; set; } = true;

    // Navigation properties
    public Gym Gym { get; set; } = null!;
    public User Alumno { get; set; } = null!;
    public Routine Rutina { get; set; } = null!;
}
