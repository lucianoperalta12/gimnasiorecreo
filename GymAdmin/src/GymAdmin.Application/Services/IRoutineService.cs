using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IRoutineService
{
    Task<PagedResult<RoutineListDto>> GetAllAsync(int requesterId, int? page = null, int? pageSize = null);
    Task<RoutineDto?> GetByIdAsync(int requesterId, int id);
    Task<RoutineDto> CreateAsync(int requesterId, CreateRoutineRequest request);
    Task<RoutineDto> UpdateAsync(int requesterId, int id, UpdateRoutineRequest request);
    Task DeleteAsync(int requesterId, int id);
}
