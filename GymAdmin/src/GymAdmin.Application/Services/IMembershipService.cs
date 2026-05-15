using GymAdmin.Application.DTOs.Memberships;

namespace GymAdmin.Application.Services;

public interface IMembershipService
{
    Task<List<MembershipListDto>> GetAllAsync(int requesterId, int? gymId = null, int? alumnoId = null, string? estado = null);
    Task<MembershipDto?> GetByIdAsync(int requesterId, int id);
    Task<List<MembershipListDto>> GetByStudentIdAsync(int requesterId, int studentId);
    Task<StudentAccessDto> GetStudentAccessAsync(int requesterId, int studentId);
    Task<StudentAccessDto> GetMyAccessAsync(int requesterId);
    Task<MembershipDto> CreateAsync(int requesterId, CreateMembershipRequest request);
    Task<MembershipDto> RenewAsync(int requesterId, int studentId, RenewMembershipRequest request);
    Task<MembershipDto> CancelAsync(int requesterId, int id, CancelMembershipRequest request);
}
