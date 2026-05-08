namespace GymAdmin.Application.DTOs.Assignments;

public record AssignRoutineRequest(int AlumnoId, int RutinaId);

public record StudentRoutineDto(
    int Id,
    int AlumnoId,
    string AlumnoNombre,
    int RutinaId,
    string RutinaNombre,
    DateTime FechaAsignacion,
    bool Activa
);

public record AssignmentSummaryDto(
    int EjerciciosCount,
    int RutinasCount,
    int AlumnosCount,
    int ProfesoresCount,
    int AsignacionesCount
);
