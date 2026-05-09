using GymAdmin.Application.DTOs.Assignments;
using GymAdmin.Application.DTOs.Routines;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _context;

    public AssignmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StudentRoutineDto> AssignAsync(AssignRoutineRequest request)
    {
        var student = await _context.Users.FindAsync(request.AlumnoId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");

        if (student.Rol != UserRole.Alumno)
            throw new InvalidOperationException("Solo se pueden asignar rutinas a alumnos.");

        var routine = await _context.Routines.FindAsync(request.RutinaId)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        var exists = await _context.StudentRoutines
            .AnyAsync(sr => sr.AlumnoId == request.AlumnoId && sr.RutinaId == request.RutinaId);

        if (exists)
            throw new InvalidOperationException("Esta rutina ya está asignada a este alumno.");

        var assignment = new StudentRoutine
        {
            AlumnoId = request.AlumnoId,
            RutinaId = request.RutinaId,
            FechaAsignacion = DateTime.UtcNow,
            Activa = true
        };

        _context.StudentRoutines.Add(assignment);
        await _context.SaveChangesAsync();

        return new StudentRoutineDto(
            assignment.Id,
            student.Id,
            student.Nombre,
            routine.Id,
            routine.Nombre,
            assignment.FechaAsignacion,
            assignment.Activa
        );
    }

    public async Task UnassignAsync(int assignmentId)
    {
        var assignment = await _context.StudentRoutines
            .Include(sr => sr.Rutina)
            .FirstOrDefaultAsync(sr => sr.Id == assignmentId)
            ?? throw new KeyNotFoundException("Asignación no encontrada.");

        _context.StudentRoutines.Remove(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<StudentRoutineDto>> GetByStudentIdAsync(int studentId)
    {
        var query = _context.StudentRoutines
            .AsNoTracking()
            .Where(sr => sr.AlumnoId == studentId)
            .Include(sr => sr.Alumno)
            .Include(sr => sr.Rutina)
            .AsQueryable();

        return await query
            .OrderByDescending(sr => sr.FechaAsignacion)
            .Select(sr => new StudentRoutineDto(
                sr.Id,
                sr.AlumnoId,
                sr.Alumno.Nombre,
                sr.RutinaId,
                sr.Rutina.Nombre,
                sr.FechaAsignacion,
                sr.Activa
            ))
            .ToListAsync();
    }

    public async Task<AssignmentSummaryDto> GetSummaryAsync()
    {
        var ejerciciosCount = await _context.Exercises
            .AsNoTracking()
            .CountAsync();

        var rutinasCount = await _context.Routines
            .AsNoTracking()
            .CountAsync();

        var alumnosCount = await _context.Users
            .AsNoTracking()
            .CountAsync(u => u.Rol == UserRole.Alumno && u.Activo);

        var profesoresCount = await _context.Users
            .AsNoTracking()
            .CountAsync(u => u.Rol == UserRole.Profesor && u.Activo);

        var asignacionesCount = await _context.StudentRoutines
            .AsNoTracking()
            .CountAsync();

        return new AssignmentSummaryDto(ejerciciosCount, rutinasCount, alumnosCount, profesoresCount, asignacionesCount);
    }

    public async Task<List<RoutineDto>> GetMyRoutinesAsync(int studentId)
    {
        var studentRoutines = await _context.StudentRoutines
            .AsNoTracking()
            .Where(sr => sr.AlumnoId == studentId && sr.Activa)
            .Include(sr => sr.Rutina)
                .ThenInclude(r => r.Profesor)
            .Include(sr => sr.Rutina)
                .ThenInclude(r => r.Ejercicios.OrderBy(e => e.Orden))
                    .ThenInclude(re => re.Ejercicio)
            .ToListAsync();

        return studentRoutines.Select(sr => new RoutineDto(
            sr.Rutina.Id,
            sr.Rutina.Nombre,
            sr.Rutina.Descripcion,
            sr.Rutina.ProfesorId,
            sr.Rutina.Profesor.Nombre,
            sr.Rutina.FechaCreacion,
            sr.FechaAsignacion,
            sr.Rutina.Activa,
            sr.Rutina.Ejercicios.Select(re => new RoutineExerciseDto(
                re.Id,
                re.EjercicioId,
                re.Ejercicio.Nombre,
                re.Ejercicio.GrupoMuscular,
                re.Ejercicio.VideoUrl,
                re.Bloque,
                re.Series,
                re.Repeticiones,
                re.Peso,
                re.DescansoSegundos,
                re.Orden,
                re.Observaciones
            )).ToList()
        )).ToList();
    }
}
