using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Payments;

namespace GymAdmin.Application.Services;

public interface IPaymentService
{
    Task<PagedResult<PaymentListDto>> GetAllAsync(int requesterId, int? gymId = null, int? membresiaId = null, int? page = null, int? pageSize = null);
    Task<PaymentDto?> GetByIdAsync(int requesterId, int id);
    Task<List<PaymentListDto>> GetByMembershipIdAsync(int requesterId, int membresiaId);
    Task<PaymentDto> CreateAsync(int requesterId, CreatePaymentRequest request);
    Task<PaymentDto> UpdateAsync(int requesterId, int id, UpdatePaymentRequest request);
    Task DeleteAsync(int requesterId, int id);
}
