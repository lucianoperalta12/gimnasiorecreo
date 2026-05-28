using GymAdmin.Application.DTOs.Exercises;
using GymAdmin.Domain.Entities;
using GymAdmin.Domain.Enums;
using GymAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Application.Services;

public class ExerciseService : IExerciseService
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public ExerciseService(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ExerciseDto>> GetAllAsync(int requesterId)
    {
        var requester = await GetRequesterAsync(requesterId);
        var query = _context.Exercises.Include(e => e.Gym).AsNoTracking();
        if (requester.Rol != UserRole.Superusuario) query = query.Where(e => e.GymId == requester.GymId);
        return await query.OrderBy(e => e.GrupoMuscular).ThenBy(e => e.Nombre).Select(e => new ExerciseDto(e.Id, e.Nombre, e.Descripcion, e.GrupoMuscular, e.VideoUrl, e.GymId, e.Gym.Nombre)).ToListAsync();
    }

    public async Task<ExerciseDto?> GetByIdAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var query = _context.Exercises.Include(e => e.Gym).AsNoTracking().Where(e => e.Id == id);
        if (requester.Rol != UserRole.Superusuario) query = query.Where(e => e.GymId == requester.GymId);
        return await query.Select(e => new ExerciseDto(e.Id, e.Nombre, e.Descripcion, e.GrupoMuscular, e.VideoUrl, e.GymId, e.Gym.Nombre)).FirstOrDefaultAsync();
    }

    public async Task<ExerciseDto> CreateAsync(int requesterId, CreateExerciseRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        var gymId = (requester.Rol == UserRole.Superusuario && request.GymId.HasValue) ? request.GymId.Value : requester.GymId;

        var exercise = new Domain.Entities.Exercise
        {
            GymId = gymId,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            GrupoMuscular = request.GrupoMuscular.Trim(),
            VideoUrl = request.VideoUrl?.Trim()
        };
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        return new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl, exercise.GymId);
    }

    public async Task<ExerciseDto> UpdateAsync(int requesterId, int id, UpdateExerciseRequest request)
    {
        var requester = await GetRequesterAsync(requesterId);
        var exercise = await _context.Exercises.FindAsync(id) ?? throw new KeyNotFoundException("Ejercicio no encontrado.");
        if (requester.Rol != UserRole.Superusuario && exercise.GymId != requester.GymId) throw new UnauthorizedAccessException();

        exercise.Nombre = request.Nombre.Trim();
        exercise.Descripcion = request.Descripcion?.Trim();
        exercise.GrupoMuscular = request.GrupoMuscular.Trim();
        exercise.VideoUrl = request.VideoUrl?.Trim();

        if (requester.Rol == UserRole.Superusuario && request.GymId.HasValue)
            exercise.GymId = request.GymId.Value;

        await _context.SaveChangesAsync();
        return new ExerciseDto(exercise.Id, exercise.Nombre, exercise.Descripcion, exercise.GrupoMuscular, exercise.VideoUrl, exercise.GymId);
    }

    public async Task DeleteAsync(int requesterId, int id)
    {
        var requester = await GetRequesterAsync(requesterId);
        var exercise = await _context.Exercises.FindAsync(id) ?? throw new KeyNotFoundException("Ejercicio no encontrado.");
        if (requester.Rol != UserRole.Superusuario && exercise.GymId != requester.GymId) throw new UnauthorizedAccessException();
        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();
    }

    private async Task<User> GetRequesterAsync(int requesterId)
    {
        var user = await _context.Users
            .Include(u => u.GymUsers)
                .ThenInclude(gu => gu.Gym)
            .FirstOrDefaultAsync(u => u.Id == requesterId)
            ?? throw new UnauthorizedAccessException();

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
                var association = user.GymUsers.FirstOrDefault(gu => gu.GymId == user.GymId && gu.Activo) ?? user.GymUsers.FirstOrDefault(gu => gu.Activo);
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
