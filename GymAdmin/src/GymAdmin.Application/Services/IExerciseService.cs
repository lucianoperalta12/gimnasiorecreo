using GymAdmin.Application.DTOs.Exercises;

namespace GymAdmin.Application.Services;

public interface IExerciseService
{
    Task<List<ExerciseDto>> GetAllAsync();
    Task<ExerciseDto?> GetByIdAsync(int id);
    Task<ExerciseDto> CreateAsync(CreateExerciseRequest request);
    Task<ExerciseDto> UpdateAsync(int id, UpdateExerciseRequest request);
    Task DeleteAsync(int id);
}
