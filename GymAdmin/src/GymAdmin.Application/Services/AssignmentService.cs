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
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public AssignmentService(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<StudentRoutineDto> AssignAsync(int requesterId, AssignRoutineRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        var student = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == request.AlumnoId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");
        var routine = await _context.Routines.FindAsync(request.RutinaId)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        var studentGymUser = student.GymUsers.FirstOrDefault(gu => gu.GymId == routine.GymId && gu.Activo);
        if (studentGymUser == null)
            throw new InvalidOperationException("El alumno y la rutina deben pertenecer al mismo gimnasio.");

        student.GymId = studentGymUser.GymId;
        student.Rol = studentGymUser.Rol;

        if (student.Rol != UserRole.Alumno)
            throw new InvalidOperationException("Solo se pueden asignar rutinas a alumnos.");
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
        var student = await _context.Users
            .Include(u => u.GymUsers)
            .FirstOrDefaultAsync(u => u.Id == studentId)
            ?? throw new KeyNotFoundException("Alumno no encontrado.");

        var studentGymUser = student.GymUsers.FirstOrDefault(gu => gu.GymId == (requester.Rol == UserRole.Superusuario ? gu.GymId : requester.GymId) && gu.Activo);
        if (studentGymUser != null)
        {
            student.GymId = studentGymUser.GymId;
            student.Rol = studentGymUser.Rol;
        }

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
            users = users.Where(u => u.GymUsers.Any(gu => gu.GymId == requester.GymId));
            assignments = assignments.Where(a => a.GymId == requester.GymId);
        }

        return new AssignmentSummaryDto(
            await exercises.CountAsync(),
            await routines.CountAsync(),
            await users.CountAsync(u => u.GymUsers.Any(gu => gu.GymId == requester.GymId && gu.Rol == UserRole.Alumno && gu.Activo) && u.Activo),
            await users.CountAsync(u => u.GymUsers.Any(gu => gu.GymId == requester.GymId && gu.Rol == UserRole.Profesor && gu.Activo) && u.Activo),
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
                re.Ejercicio.Descripcion,
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

    private async Task<User> GetRequesterAsync(int requesterId)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == requesterId)
            ?? throw new UnauthorizedAccessException("Usuario invalido.");

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var gymIdClaim = httpContext.User.FindFirst("gymId")?.Value;
            if (int.TryParse(gymIdClaim, out var gymId))
            {
                user.GymId = gymId;
            }

            var roleClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (Enum.TryParse<UserRole>(roleClaim, true, out var role))
            {
                user.Rol = role;
            }
            else
            {
                var association = user.GymUsers.FirstOrDefault(gu => gu.GymId == user.GymId && gu.Activo)
                    ?? user.GymUsers.FirstOrDefault(gu => gu.Activo);
                if (association != null)
                {
                    user.Rol = association.Rol;
                    if (user.GymId == 0)
                    {
                        user.GymId = association.GymId;
                    }
                }
            }
        }

        return user;
    }
}
