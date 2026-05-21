using GymAdmin.Application.DTOs.Ingresos;

namespace GymAdmin.Application.Services;

public interface IIngresoService
{
    Task<RegistrarIngresoResponse> RegistrarAsync(int terminalUserId, RegistrarIngresoRequest request);
    Task<List<IngresoListItemDto>> GetAllAsync(int requesterId, DateOnly? fecha = null, int? alumnoId = null, int? gymId = null);
}
