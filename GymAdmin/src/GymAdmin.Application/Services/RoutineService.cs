using GymAdmin.Application.DTOs.Routines;
using GymAdmin.Domain.Entities;
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

    public async Task<List<RoutineListDto>> GetAllAsync()
    {
        var query = _context.Routines
            .AsNoTracking()
            .Include(r => r.Profesor)
            .Include(r => r.Ejercicios)
            .AsQueryable();

        return await query
            .OrderByDescending(r => r.FechaCreacion)
            .Select(r => new RoutineListDto(
                r.Id,
                r.Nombre,
                r.Descripcion,
                r.Profesor.Nombre,
                r.FechaCreacion,
                r.Activa,
                r.Ejercicios.Count
            ))
            .ToListAsync();
    }

    public async Task<RoutineDto?> GetByIdAsync(int id, int? studentId = null)
    {
        var routine = await _context.Routines
            .AsNoTracking()
            .Include(r => r.Profesor)
            .Include(r => r.Ejercicios.OrderBy(e => e.Orden))
                .ThenInclude(re => re.Ejercicio)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (routine is null) return null;

        DateTime? fechaAsignacion = null;
        if (studentId.HasValue)
        {
            fechaAsignacion = await _context.StudentRoutines
                .Where(sr => sr.RutinaId == id && sr.AlumnoId == studentId.Value)
                .Select(sr => sr.FechaAsignacion)
                .FirstOrDefaultAsync();
        }

        return MapToDto(routine, fechaAsignacion);
    }

    public async Task<RoutineDto> CreateAsync(int profesorId, CreateRoutineRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre de la rutina es obligatorio.");

        if (request.Ejercicios is null || request.Ejercicios.Count == 0)
            throw new ArgumentException("La rutina debe tener al menos un ejercicio.");

        ValidateExerciseSections(request.Ejercicios);

        // Validate exercise IDs exist
        var exerciseIds = request.Ejercicios.Select(e => e.EjercicioId).Distinct().ToList();
        var existingCount = await _context.Exercises.CountAsync(e => exerciseIds.Contains(e.Id));
        if (existingCount != exerciseIds.Count)
            throw new ArgumentException("Uno o más ejercicios no existen.");

        var routine = new Routine
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            ProfesorId = profesorId,
            Activa = true,
            FechaCreacion = DateTime.UtcNow,
            Ejercicios = request.Ejercicios.Select(e => new RoutineExercise
            {
                EjercicioId = e.EjercicioId,
                Bloque = NormalizeSection(e.Bloque),
                Series = e.Series,
                Repeticiones = e.Repeticiones,
                Peso = e.Peso,
                DescansoSegundos = e.DescansoSegundos,
                Orden = e.Orden,
                Observaciones = e.Observaciones?.Trim()
            }).ToList()
        };

        _context.Routines.Add(routine);
        await _context.SaveChangesAsync();

        // Reload with includes
        return (await GetByIdAsync(routine.Id))!;
    }

    public async Task<RoutineDto> UpdateAsync(int id, UpdateRoutineRequest request)
    {
        var routine = await _context.Routines
            .Include(r => r.Ejercicios)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre de la rutina es obligatorio.");

        routine.Nombre = request.Nombre.Trim();
        routine.Descripcion = request.Descripcion?.Trim();
        routine.Activa = request.Activa;

        // Replace exercises: remove old, add new
        _context.RoutineExercises.RemoveRange(routine.Ejercicios);

        if (request.Ejercicios is not null && request.Ejercicios.Count > 0)
        {
            ValidateExerciseSections(request.Ejercicios);

            var exerciseIds = request.Ejercicios.Select(e => e.EjercicioId).Distinct().ToList();
            var existingCount = await _context.Exercises.CountAsync(e => exerciseIds.Contains(e.Id));
            if (existingCount != exerciseIds.Count)
                throw new ArgumentException("Uno o más ejercicios no existen.");

            routine.Ejercicios = request.Ejercicios.Select(e => new RoutineExercise
            {
                RutinaId = routine.Id,
                EjercicioId = e.EjercicioId,
                Bloque = NormalizeSection(e.Bloque),
                Series = e.Series,
                Repeticiones = e.Repeticiones,
                Peso = e.Peso,
                DescansoSegundos = e.DescansoSegundos,
                Orden = e.Orden,
                Observaciones = e.Observaciones?.Trim()
            }).ToList();
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(routine.Id))!;
    }

    public async Task DeleteAsync(int id)
    {
        var routine = await _context.Routines.FindAsync(id)
            ?? throw new KeyNotFoundException("Rutina no encontrada.");

        _context.Routines.Remove(routine);
        await _context.SaveChangesAsync();
    }

    private static RoutineDto MapToDto(Routine routine, DateTime? fechaAsignacion = null)
    {
        return new RoutineDto(
            routine.Id,
            routine.Nombre,
            routine.Descripcion,
            routine.ProfesorId,
            routine.Profesor.Nombre,
            routine.FechaCreacion,
            fechaAsignacion,
            routine.Activa,
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
                re.Observaciones
            )).ToList()
        );
    }

    private static void ValidateExerciseSections(IEnumerable<CreateRoutineExerciseRequest> ejercicios)
    {
        if (ejercicios.Any(e => !AllowedSections.Contains(NormalizeSection(e.Bloque))))
            throw new ArgumentException("Cada ejercicio debe pertenecer a calentamiento inicial, parte media o fuerza.");
    }

    private static string NormalizeSection(string? bloque)
    {
        return string.IsNullOrWhiteSpace(bloque)
            ? string.Empty
            : bloque.Trim();
    }

    private static readonly string[] AllowedSections =
    [
        RoutineExerciseSectionLabels.CalentamientoInicial,
        RoutineExerciseSectionLabels.ParteMedia,
        RoutineExerciseSectionLabels.Fuerza
    ];
}
