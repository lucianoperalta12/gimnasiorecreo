namespace GymAdmin.Domain.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string GrupoMuscular { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }

    // Navigation properties
    public ICollection<RoutineExercise> RutinaEjercicios { get; set; } = new List<RoutineExercise>();
}
