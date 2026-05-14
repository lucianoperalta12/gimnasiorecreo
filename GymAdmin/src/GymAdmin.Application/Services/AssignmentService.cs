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

    public async Task<StudentRoutineDto> AssignAsync(int requesterId, AssignRoutineRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        var student = await _context.Users.FindAsync(request.AlumnoId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");
        var routine = await _context.Routines.FindAsync(request.RutinaId)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        if (student.Rol != UserRole.Alumno)
            throw new InvalidOperationException("Solo se pueden asignar rutinas a alumnos.");
        if (student.GymId != routine.GymId)
            throw new InvalidOperationException("El alumno y la rutina deben pertenecer al mismo gimnasio.");
        if (requester.Rol != UserRole.Superusuario && requester.GymId != student.GymId)
            throw new UnauthorizedAccessException("No autorizado.");

        var exists = await _context.StudentRoutines
            .AnyAsync(sr => sr.AlumnoId == request.AlumnoId && sr.RutinaId == request.RutinaId);
        if (exists)
            throw new InvalidOperationException("Esta rutina ya esta asignada a este alumno.");

        var assignment = new StudentRoutine
        {
            GymId = student.GymId,
            AlumnoId = request.AlumnoId,
            RutinaId = request.RutinaId,
            FechaAsignacion = DateTime.UtcNow,
            Activa = true
        };

        _context.StudentRoutines.Add(assignment);
        await _context.SaveChangesAsync();

        return new StudentRoutineDto(assignment.Id, student.Id, student.Nombre, routine.Id, routine.Nombre, assignment.FechaAsignacion, assignment.Activa);
    }

    public async Task UnassignAsync(int requesterId, int assignmentId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var assignment = await _context.StudentRoutines.FirstOrDefaultAsync(sr => sr.Id == assignmentId)
            ?? throw new KeyNotFoundException("Asignacion no encontrada.");

        if (requester.Rol != UserRole.Superusuario && requester.GymId != assignment.GymId)
            throw new UnauthorizedAccessException("No autorizado.");

        _context.StudentRoutines.Remove(assignment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<StudentRoutineDto>> GetByStudentIdAsync(int requesterId, int studentId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var student = await _context.Users.FindAsync(studentId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");

        if (requester.Rol != UserRole.Superusuario && requester.GymId != student.GymId)
            throw new UnauthorizedAccessException("No autorizado.");

        return await _context.StudentRoutines
            .AsNoTracking()
            .Where(sr => sr.AlumnoId == studentId)
            .Include(sr => sr.Alumno)
            .Include(sr => sr.Rutina)
            .OrderByDescending(sr => sr.FechaAsignacion)
            .Select(sr => new StudentRoutineDto(sr.Id, sr.AlumnoId, sr.Alumno.Nombre, sr.RutinaId, sr.Rutina.Nombre, sr.FechaAsignacion, sr.Activa))
            .ToListAsync();
    }

    public async Task<AssignmentSummaryDto> GetSummaryAsync(int requesterId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var exercises = _context.Exercises.AsNoTracking().AsQueryable();
        var routines = _context.Routines.AsNoTracking().AsQueryable();
        var users = _context.Users.AsNoTracking().AsQueryable();
        var assignments = _context.StudentRoutines.AsNoTracking().AsQueryable();

        if (requester.Rol != UserRole.Superusuario)
        {
            exercises = exercises.Where(e => e.GymId == requester.GymId);
            routines = routines.Where(r => r.GymId == requester.GymId);
            users = users.Where(u => u.GymId == requester.GymId);
            assignments = assignments.Where(a => a.GymId == requester.GymId);
        }

        return new AssignmentSummaryDto(
            await exercises.CountAsync(),
            await routines.CountAsync(),
            await users.CountAsync(u => u.Rol == UserRole.Alumno && u.Activo),
            await users.CountAsync(u => u.Rol == UserRole.Profesor && u.Activo),
            await assignments.CountAsync()
        );
    }

    public async Task<List<RoutineDto>> GetMyRoutinesAsync(int studentId)
    {
        var student = await GetRequesterAsync(studentId);
        var studentRoutines = await _context.StudentRoutines
            .AsNoTracking()
            .Where(sr => sr.AlumnoId == studentId && sr.GymId == student.GymId && sr.Activa)
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
            sr.Rutina.IsByDays,
            sr.Rutina.DaysCount,
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
                re.Observaciones,
                re.DayNumber
            )).ToList()
        )).ToList();
    }

    private async Task<User> GetRequesterAsync(int requesterId) =>
        await _context.Users.FindAsync(requesterId)
        ?? throw new UnauthorizedAccessException("Usuario invalido.");
}
