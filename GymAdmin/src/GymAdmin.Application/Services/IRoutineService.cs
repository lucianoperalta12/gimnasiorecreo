using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IRoutineService
{
    Task<List<RoutineListDto>> GetAllAsync(int requesterId);
    Task<RoutineDto?> GetByIdAsync(int requesterId, int id);
    Task<RoutineDto> CreateAsync(int requesterId, CreateRoutineRequest request);
    Task<RoutineDto> UpdateAsync(int requesterId, int id, UpdateRoutineRequest request);
    Task DeleteAsync(int requesterId, int id);
}
