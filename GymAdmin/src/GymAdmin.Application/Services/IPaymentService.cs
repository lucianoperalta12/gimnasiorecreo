using GymAdmin.Application.DTOs.Payments;

namespace GymAdmin.Application.Services;

public interface IPaymentService
{
    Task<List<PaymentListDto>> GetAllAsync(int requesterId, int? gymId = null, int? membresiaId = null);
    Task<PaymentDto?> GetByIdAsync(int requesterId, int id);
    Task<List<PaymentListDto>> GetByMembershipIdAsync(int requesterId, int membresiaId);
    Task<PaymentDto> CreateAsync(int requesterId, CreatePaymentRequest request);
    Task<PaymentDto> UpdateAsync(int requesterId, int id, UpdatePaymentRequest request);
    Task DeleteAsync(int requesterId, int id);
}
