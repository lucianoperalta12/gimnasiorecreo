using GymAdmin.Application.DTOs.Exercises;

namespace GymAdmin.Application.Services;

public interface IExerciseService
{
    Task<List<ExerciseDto>> GetAllAsync(int requesterId);
    Task<ExerciseDto?> GetByIdAsync(int requesterId, int id);
    Task<ExerciseDto> CreateAsync(int requesterId, CreateExerciseRequest request);
    Task<ExerciseDto> UpdateAsync(int requesterId, int id, UpdateExerciseRequest request);
    Task DeleteAsync(int requesterId, int id);
}
