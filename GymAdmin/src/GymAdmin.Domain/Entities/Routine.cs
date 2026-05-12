namespace GymAdmin.Domain.Entities;

public class Routine
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int ProfesorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activa { get; set; } = true;
    public bool IsByDays { get; set; } = false;
    public int DaysCount { get; set; } = 1;

    // Navigation properties
    public User Profesor { get; set; } = null!;
    public ICollection<RoutineExercise> Ejercicios { get; set; } = new List<RoutineExercise>();
    public ICollection<StudentRoutine> AlumnosAsignados { get; set; } = new List<StudentRoutine>();
}
