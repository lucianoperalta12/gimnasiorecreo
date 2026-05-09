using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IRoutineService
{
    Task<List<RoutineListDto>> GetAllAsync();
    Task<RoutineDto?> GetByIdAsync(int id, int? studentId = null);
    Task<RoutineDto> CreateAsync(int profesorId, CreateRoutineRequest request);
    Task<RoutineDto> UpdateAsync(int id, UpdateRoutineRequest request);
    Task DeleteAsync(int id);
}
