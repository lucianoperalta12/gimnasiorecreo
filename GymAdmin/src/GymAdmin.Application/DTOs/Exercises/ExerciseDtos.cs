namespace GymAdmin.Application.DTOs.Exercises;

public record ExerciseDto(int Id, string Nombre, string? Descripcion, string GrupoMuscular, string? VideoUrl, int GymId = 0, string? GymNombre = null);

public record CreateExerciseRequest(string Nombre, string? Descripcion, string GrupoMuscular, string? VideoUrl);
public record UpdateExerciseRequest(string Nombre, string? Descripcion, string GrupoMuscular, string? VideoUrl);
