using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Ingresos;

namespace GymAdmin.Application.Services;

public interface IIngresoService
{
    Task<RegistrarIngresoResponse> RegistrarAsync(int terminalUserId, RegistrarIngresoRequest request);
    Task<PagedResult<IngresoListItemDto>> GetAllAsync(int requesterId, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null, int? alumnoId = null, int? gymId = null, int? page = null, int? pageSize = null);
    Task<List<IngresoHoyItemDto>> GetTodayAsync(int requesterId, int? gymId = null);
}
