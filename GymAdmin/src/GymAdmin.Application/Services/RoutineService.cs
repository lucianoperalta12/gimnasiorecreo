using GymAdmin.Application.DTOs.Routines;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class RoutineService : IRoutineService
{
    private readonly AppDbContext _context;

    public RoutineService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoutineListDto>> GetAllAsync(int requesterId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var query = _context.Routines
            .AsNoTracking()
            .Include(r => r.Profesor)
            .Include(r => r.Gym)
            .Include(r => r.Ejercicios)
            .AsQueryable();

        if (requester.Rol != UserRole.Superusuario)
            query = query.Where(r => r.GymId == requester.GymId);

        return await query
            .OrderByDescending(r => r.FechaCreacion)
            .Select(r => new RoutineListDto(
                r.Id,
                r.Nombre,
                r.Descripcion,
                r.Profesor.Nombre,
                r.FechaCreacion,
                r.Activa,
                r.Ejercicios.Count,
                r.GymId,
                r.Gym.Nombre
            ))
            .ToListAsync();
    }

    public async Task<RoutineDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var routine = await _context.Routines
            .AsNoTracking()
            .Include(r => r.Profesor)
            .Include(r => r.AlumnosAsignados)
            .Include(r => r.Ejercicios.OrderBy(e => e.Orden))
                .ThenInclude(re => re.Ejercicio)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (routine is null || !CanAccessRoutine(requester, routine))
            return null;

        DateTime? fechaAsignacion = null;
        if (requester.Rol == UserRole.Alumno || requester.Rol == UserRole.Administrativo)
        {
            fechaAsignacion = await _context.StudentRoutines
                .Where(sr => sr.RutinaId == id && sr.AlumnoId == requester.Id)
                .Select(sr => sr.FechaAsignacion)
                .FirstOrDefaultAsync();
        }

        return MapToDto(routine, fechaAsignacion);
    }

    public async Task<RoutineDto> CreateAsync(int requesterId, CreateRoutineRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        if (requester.Rol == UserRole.Alumno)
            throw new UnauthorizedAccessException("No autorizado.");

        var gymId = (requester.Rol == UserRole.Superusuario && request.GymId.HasValue) ? request.GymId.Value : requester.GymId;

        ValidateRequest(request);
        await ValidateExercisesBelongToGymAsync(request.Ejercicios, gymId);

        var routine = new Routine
        {
            GymId = gymId,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            ProfesorId = requester.Id,
            Activa = true,
            IsByDays = request.IsByDays,
            DaysCount = request.IsByDays ? request.DaysCount : 1,
            FechaCreacion = DateTime.UtcNow,
            Ejercicios = BuildRoutineExercises(request)
        };

        _context.Routines.Add(routine);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(requesterId, routine.Id))!;
    }

    public async Task<RoutineDto> UpdateAsync(int requesterId, int id, UpdateRoutineRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        var routine = await _context.Routines
            .Include(r => r.Ejercicios)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        if (requester.Rol != UserRole.Superusuario && routine.GymId != requester.GymId)
            throw new UnauthorizedAccessException("No autorizado.");

        var targetGymId = (requester.Rol == UserRole.Superusuario && request.GymId.HasValue) ? request.GymId.Value : routine.GymId;

        ValidateRequest(request);
        await ValidateExercisesBelongToGymAsync(request.Ejercicios, targetGymId);

        routine.Nombre = request.Nombre.Trim();
        routine.Descripcion = request.Descripcion?.Trim();
        routine.Activa = request.Activa;
        routine.IsByDays = request.IsByDays;
        routine.DaysCount = request.IsByDays ? request.DaysCount : 1;
        routine.GymId = targetGymId;

        _context.RoutineExercises.RemoveRange(routine.Ejercicios);
        routine.Ejercicios = BuildRoutineExercises(request, routine.Id);

        await _context.SaveChangesAsync();
        return (await GetByIdAsync(requesterId, routine.Id))!;
    }

    public async Task DeleteAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var routine = await _context.Routines.FindAsync(id)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        if (requester.Rol != UserRole.Superusuario && routine.GymId != requester.GymId)
            throw new UnauthorizedAccessException("No autorizado.");

        _context.Routines.Remove(routine);
        await _context.SaveChangesAsync();
    }

    private async Task<User> GetRequesterAsync(int requesterId) =>
        await _context.Users.FindAsync(requesterId)
        ?? throw new UnauthorizedAccessException("Usuario invalido.");

    private static bool CanAccessRoutine(User requester, Routine routine)
    {
        if (requester.Rol == UserRole.Superusuario) return true;
        if (routine.GymId != requester.GymId) return false;
        if (requester.Rol == UserRole.Alumno)
            return routine.AlumnosAsignados.Any(sr => sr.AlumnoId == requester.Id);
        return true;
    }

    private async Task ValidateExercisesBelongToGymAsync(List<CreateRoutineExerciseRequest> ejercicios, int gymId)
    {
        var exerciseIds = ejercicios.Select(e => e.EjercicioId).Distinct().ToList();
        var existingCount = await _context.Exercises.CountAsync(e => exerciseIds.Contains(e.Id) && e.GymId == gymId);
        if (existingCount != exerciseIds.Count)
            throw new ArgumentException("Uno o mas ejercicios no existen o pertenecen a otro gimnasio.");
    }

    private static void ValidateRequest(CreateRoutineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre de la rutina es obligatorio.");
        if (request.Ejercicios is null || request.Ejercicios.Count == 0)
            throw new ArgumentException("La rutina debe tener al menos un ejercicio.");
        ValidateExerciseSections(request.Ejercicios);
    }

    private static void ValidateRequest(UpdateRoutineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre de la rutina es obligatorio.");
        if (request.Ejercicios is null || request.Ejercicios.Count == 0)
            throw new ArgumentException("La rutina debe tener al menos un ejercicio.");
        ValidateExerciseSections(request.Ejercicios);
    }

    private static List<RoutineExercise> BuildRoutineExercises(CreateRoutineRequest request) =>
        request.Ejercicios.Select(e => BuildRoutineExercise(e, request.IsByDays)).ToList();

    private static List<RoutineExercise> BuildRoutineExercises(UpdateRoutineRequest request, int routineId) =>
        request.Ejercicios.Select(e =>
        {
            var re = BuildRoutineExercise(e, request.IsByDays);
            re.RutinaId = routineId;
            return re;
        }).ToList();

    private static RoutineExercise BuildRoutineExercise(CreateRoutineExerciseRequest e, bool isByDays) => new()
    {
        EjercicioId = e.EjercicioId,
        Bloque = NormalizeSection(e.Bloque),
        Series = e.Series,
        Repeticiones = e.Repeticiones,
        Peso = e.Peso,
        DescansoSegundos = e.DescansoSegundos,
        Orden = e.Orden,
        Observaciones = e.Observaciones?.Trim(),
        DayNumber = isByDays ? e.DayNumber : 1
    };

    private static RoutineDto MapToDto(Routine routine, DateTime? fechaAsignacion = null) => new(
        routine.Id,
        routine.Nombre,
        routine.Descripcion,
        routine.ProfesorId,
        routine.Profesor.Nombre,
        routine.FechaCreacion,
        fechaAsignacion,
        routine.Activa,
        routine.IsByDays,
        routine.DaysCount,
        routine.Ejercicios.Select(re => new RoutineExerciseDto(
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
        )).ToList(),
        routine.GymId
    );

    private static void ValidateExerciseSections(IEnumerable<CreateRoutineExerciseRequest> ejercicios)
    {
        if (ejercicios.Any(e => !AllowedSections.Contains(NormalizeSection(e.Bloque))))
            throw new ArgumentException("Cada ejercicio debe pertenecer a calentamiento inicial, bloque central o movilidad.");
    }

    private static string NormalizeSection(string? bloque) =>
        string.IsNullOrWhiteSpace(bloque) ? string.Empty : bloque.Trim();

    private static readonly string[] AllowedSections =
    [
        RoutineExerciseSectionLabels.CalentamientoInicial,
        RoutineExerciseSectionLabels.ParteMedia,
        RoutineExerciseSectionLabels.Fuerza
    ];
}
