using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Egresos;

namespace GymAdmin.Application.Services;

public interface IEgresoService
{
    Task<PagedResult<EgresoListDto>> GetAllAsync(int requesterId, int? gymId = null, int? page = null, int? pageSize = null);
    Task<EgresoDto?> GetByIdAsync(int requesterId, int id);
    Task<EgresoDto> CreateAsync(int requesterId, CreateEgresoRequest request);
    Task<EgresoDto> UpdateAsync(int requesterId, int id, UpdateEgresoRequest request);
    Task DeleteAsync(int requesterId, int id);
}
