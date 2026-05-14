using GymAdmin.Application.DTOs.Exercises;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class ExerciseService : IExerciseService
{
    private readonly AppDbContext _context;
    public ExerciseService(AppDbContext context) { _context = context; }

    public async Task<List<ExerciseDto>> GetAllAsync(int requesterId)
    {
        var requester = await _context.Users.FindAsync(requesterId) ?? throw new UnauthorizedAccessException();
        var query = _context.Exercises.Include(e => e.Gym).AsNoTracking();
        if (requester.Rol != UserRole.Superusuario) query = query.Where(e => e.GymId == requester.GymId);
        return await query.OrderBy(e => e.GrupoMuscular).ThenBy(e => e.Nombre).Select(e => new ExerciseDto(e.Id, e.Nombre, e.Descripcion, e.GrupoMuscular, e.VideoUrl, e.GymId, e.Gym.Nombre)).ToListAsync();
    }

    public async Task<ExerciseDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await _context.Users.FindAsync(requesterId) ?? throw new UnauthorizedAccessException();
        var query = _context.Exercises.Include(e => e.Gym).AsNoTracking().Where(e => e.Id == id);
        if (requester.Rol != UserRole.Superusuario) query = query.Where(e => e.GymId == requester.GymId);
        return await query.Select(e => new ExerciseDto(e.Id, e.Nombre, e.Descripcion, e.GrupoMuscular, e.VideoUrl, e.GymId, e.Gym.Nombre)).FirstOrDefaultAsync();
    }

    public async Task<ExerciseDto> CreateAsync(int requesterId, CreateExerciseRequest request)
    {
        var requester = await _context.Users.FindAsync(requesterId) ?? throw new UnauthorizedAccessException();
        var exercise = new Domain.Entities.Exercise
        {
            GymId = requester.GymId,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            GrupoMuscular = request.GrupoMuscular.Trim(),
            VideoUrl = request.VideoUrl?.Trim()
        };
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl);
    }

    public async Task<ExerciseDto> UpdateAsync(int requesterId, int id, UpdateExerciseRequest request)
    {
        var requester = await _context.Users.FindAsync(requesterId) ?? throw new UnauthorizedAccessException();
        var exercise = await _context.Exercises.FindAsync(id) ?? throw new KeyNotFoundException("Ejercicio no encontrado.");
        if (requester.Rol != UserRole.Superusuario && exercise.GymId != requester.GymId) throw new UnauthorizedAccessException();
        exercise.Nombre = request.Nombre.Trim();
        exercise.Descripcion = request.Descripcion?.Trim();
        exercise.GrupoMuscular = request.GrupoMuscular.Trim();
        exercise.VideoUrl = request.VideoUrl?.Trim();
        await _context.SaveChangesAsync();
        return new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl);
    }

    public async Task DeleteAsync(int requesterId, int id)
    {
        var requester = await _context.Users.FindAsync(requesterId) ?? throw new UnauthorizedAccessException();
        var exercise = await _context.Exercises.FindAsync(id) ?? throw new KeyNotFoundException("Ejercicio no encontrado.");
        if (requester.Rol != UserRole.Superusuario && exercise.GymId != requester.GymId) throw new UnauthorizedAccessException();
        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
    }
}
