namespace GymAdmin.Domain.Entities;

public class Exercise
{
    public int Id { get; set; }
    public int GymId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string GrupoMuscular { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }

    // Navigation properties
    public Gym Gym { get; set; } = null!;
    public ICollection<RoutineExercise> RutinaEjercicios { get; set; } = new List<RoutineExercise>();
}
