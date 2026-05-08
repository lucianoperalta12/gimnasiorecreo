using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IRoutineService
{
    Task<List<RoutineListDto>> GetAllAsync(int? profesorId = null);
    Task<RoutineDto?> GetByIdAsync(int id, int? studentId = null);
    Task<RoutineDto> CreateAsync(int profesorId, CreateRoutineRequest request);
    Task<RoutineDto> UpdateAsync(int id, int profesorId, UpdateRoutineRequest request);
    Task DeleteAsync(int id, int profesorId);
}
