using GymAdmin.Application.DTOs.MembershipPlans;

namespace GymAdmin.Application.Services;

public interface IMembershipPlanService
{
    Task<List<MembershipPlanDto>> GetAllAsync(int requesterId, int? gymId = null);
    Task<MembershipPlanDto?> GetByIdAsync(int requesterId, int id);
    Task<MembershipPlanDto> CreateAsync(int requesterId, CreateMembershipPlanRequest request);
    Task<MembershipPlanDto> UpdateAsync(int requesterId, int id, UpdateMembershipPlanRequest request);
    Task DeleteAsync(int requesterId, int id);
}
