namespace GymAdmin.Application.DTOs.Routines;

public record RoutineDto(
    int Id,
    string Nombre,
    string? Descripcion,
    int ProfesorId,
    string ProfesorNombre,
    DateTime FechaCreacion,
    DateTime? FechaAsignacion,
    bool Activa,
    bool IsByDays,
    int DaysCount,
    List<RoutineExerciseDto> Ejercicios
);

public record RoutineListDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string ProfesorNombre,
    DateTime FechaCreacion,
    bool Activa,
    int CantidadEjercicios
);

public record RoutineExerciseDto(
    int Id,
    int EjercicioId,
    string EjercicioNombre,
    string GrupoMuscular,
    string? VideoUrl,
    string Bloque,
    int Series,
    int Repeticiones,
    decimal? Peso,
    int? DescansoSegundos,
    int Orden,
    string? Observaciones,
    int DayNumber = 1
);

public record CreateRoutineRequest(
    string Nombre,
    string? Descripcion,
    bool IsByDays,
    int DaysCount,
    List<CreateRoutineExerciseRequest> Ejercicios
);

public record CreateRoutineExerciseRequest(
    int EjercicioId,
    string Bloque,
    int Series,
    int Repeticiones,
    decimal? Peso,
    int? DescansoSegundos,
    int Orden,
    string? Observaciones,
    int DayNumber = 1
);

public record UpdateRoutineRequest(
    string Nombre,
    string? Descripcion,
    bool Activa,
    bool IsByDays,
    int DaysCount,
    List<CreateRoutineExerciseRequest> Ejercicios
);
