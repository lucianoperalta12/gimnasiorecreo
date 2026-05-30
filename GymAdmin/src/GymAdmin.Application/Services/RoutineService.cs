using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Routines;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class RoutineService : IRoutineService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public RoutineService(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResult<RoutineListDto>> GetAllAsync(int requesterId, int? page = null, int? pageSize = null)
    {
        var requester = await GetRequesterAsync(requesterId);
        var query = _context.Routines.AsNoTracking().AsQueryable();

        if (requester.Rol != UserRole.Superusuario)
            query = query.Where(r => r.GymId == requester.GymId);

        var totalCount = await query.CountAsync();
        var pagedQuery = ApplyPagination(query.OrderByDescending(r => r.FechaCreacion), page, pageSize);

        var items = await pagedQuery
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

        return new PagedResult<RoutineListDto>(items, totalCount, page, NormalizePageSize(pageSize));
    }

    public async Task<RoutineDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var query = _context.Routines
            .AsNoTracking()
            .Where(r => r.Id == id);

        if (requester.Rol != UserRole.Superusuario)
            query = query.Where(r => r.GymId == requester.GymId);

        if (requester.Rol == UserRole.Alumno)
            query = query.Where(r => r.AlumnosAsignados.Any(sr => sr.AlumnoId == requester.Id));

        var routine = await query
            .Select(r => new RoutineDto(
                r.Id,
                r.Nombre,
                r.Descripcion,
                r.ProfesorId,
                r.Profesor.Nombre,
                r.FechaCreacion,
                null,
                r.Activa,
                r.IsByDays,
                r.DaysCount,
                r.Ejercicios
                    .OrderBy(re => re.Orden)
                    .Select(re => new RoutineExerciseDto(
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
                    ))
                    .ToList(),
                r.GymId
            ))
            .FirstOrDefaultAsync();

        if (routine is null)
            return null;

        DateTime? fechaAsignacion = null;
        if (requester.Rol == UserRole.Alumno || requester.Rol == UserRole.Administrativo)
        {
            fechaAsignacion = await _context.StudentRoutines
                .Where(sr => sr.RutinaId == id && sr.AlumnoId == requester.Id)
                .Select(sr => sr.FechaAsignacion)
                .FirstOrDefaultAsync();
        }

        return routine with { FechaAsignacion = fechaAsignacion };
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
        }

        return user;
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

    private static IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int? page, int? pageSize)
    {
        var normalizedPageSize = NormalizePageSize(pageSize);
        if (!normalizedPageSize.HasValue)
            return query;

        var normalizedPage = NormalizePage(page);
        return query.Skip((normalizedPage - 1) * normalizedPageSize.Value).Take(normalizedPageSize.Value);
    }

    private static int NormalizePage(int? page) => page.GetValueOrDefault(1) > 0 ? page.GetValueOrDefault(1) : 1;

    private static int? NormalizePageSize(int? pageSize)
    {
        if (!pageSize.HasValue || pageSize.Value <= 0)
            return null;

        return Math.Min(pageSize.Value, 200);
    }
}
