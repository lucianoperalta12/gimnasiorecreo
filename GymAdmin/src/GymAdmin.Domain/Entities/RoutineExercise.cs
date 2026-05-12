namespace GymAdmin.Domain.Entities;

public class RoutineExercise
{
    public int Id { get; set; }
    public int RutinaId { get; set; }
    public int EjercicioId { get; set; }
    public string Bloque { get; set; } = RoutineExerciseSectionLabels.ParteMedia;
    public int Series { get; set; }
    public int Repeticiones { get; set; }
    public decimal? Peso { get; set; }
    public int? DescansoSegundos { get; set; }
    public int Orden { get; set; }
    public string? Observaciones { get; set; }
    public int DayNumber { get; set; } = 1;

    // Navigation properties
    public Routine Rutina { get; set; } = null!;
    public Exercise Ejercicio { get; set; } = null!;
}
