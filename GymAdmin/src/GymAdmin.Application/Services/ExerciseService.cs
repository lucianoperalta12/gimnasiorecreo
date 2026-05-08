using GymAdmin.Application.DTOs.Exercises;
using GymAdmin.Domain.Entities;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class ExerciseService : IExerciseService
{
    private readonly AppDbContext _context;

    public ExerciseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExerciseDto>> GetAllAsync()
    {
        return await _context.Exercises
            .AsNoTracking()
            .OrderBy(e => e.GrupoMuscular)
            .ThenBy(e => e.Nombre)
            .Select(e => new ExerciseDto(e.Id, e.Nombre, e.Descripcion, e.GrupoMuscular, e.VideoUrl))
            .ToListAsync();
    }

    public async Task<ExerciseDto?> GetByIdAsync(int id)
    {
        var exercise = await _context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return exercise is null
            ? null
            : new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl);
    }

    public async Task<ExerciseDto> CreateAsync(CreateExerciseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre del ejercicio es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.GrupoMuscular))
            throw new ArgumentException("El grupo muscular es obligatorio.");

        var exercise = new Exercise
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            GrupoMuscular = request.GrupoMuscular.Trim(),
            VideoUrl = request.VideoUrl?.Trim()
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        return new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl);
    }

    public async Task<ExerciseDto> UpdateAsync(int id, UpdateExerciseRequest request)
    {
        var exercise = await _context.Exercises.FindAsync(id)
            ?? throw new KeyNotFoundException("Ejercicio no encontrado.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ArgumentException("El nombre del ejercicio es obligatorio.");

        exercise.Nombre = request.Nombre.Trim();
        exercise.Descripcion = request.Descripcion?.Trim();
        exercise.GrupoMuscular = request.GrupoMuscular.Trim();
        exercise.VideoUrl = request.VideoUrl?.Trim();

        await _context.SaveChangesAsync();

        return new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl);
    }

    public async Task DeleteAsync(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id)
            ?? throw new KeyNotFoundException("Ejercicio no encontrado.");

        var isUsed = await _context.RoutineExercises.AnyAsync(re => re.EjercicioId == id);
        if (isUsed)
            throw new InvalidOperationException("No se puede eliminar un ejercicio que está siendo usado en rutinas.");

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
    }
}
