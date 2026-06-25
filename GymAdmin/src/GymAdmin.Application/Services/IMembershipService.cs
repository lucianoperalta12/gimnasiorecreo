using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Memberships;
using GymAdmin.Domain.Entities;

namespace GymAdmin.Application.Services;

public interface IMembershipService
{
    Task<PagedResult<MembershipListDto>> GetAllAsync(
        int requesterId,
        int? gymId = null,
        int? alumnoId = null,
        string? estado = null,
        string? search = null,
        string? sortBy = null,
        bool? sortDesc = null,
        int? page = null,
        int? pageSize = null,
        DateTime? fechaVencimientoDesde = null,
        DateTime? fechaVencimientoHasta = null,
        bool? sinActiva = null);
    Task<DashboardMembershipSummaryDto> GetDashboardSummaryAsync(int requesterId, int? gymId);
    Task<MembershipDto?> GetByIdAsync(int requesterId, int id);
    Task<List<MembershipListDto>> GetByStudentIdAsync(int requesterId, int studentId);
    Task<StudentAccessDto> GetStudentAccessAsync(int requesterId, int studentId);
    Task<StudentAccessDto> GetMyAccessAsync(int requesterId);
    Task<MembershipDto> CreateAsync(int requesterId, CreateMembershipRequest request);
    Task<MembershipDto> RenewAsync(int requesterId, int studentId, RenewMembershipRequest request);
    Task<MembershipDto> CancelAsync(int requesterId, int id, CancelMembershipRequest request);
    Task SendExpirationEmailManualAsync(int requesterId, int id);
    Task SendExpirationEmailAsync(Membership m, bool throwOnError = false);
    Task<List<MembershipRenovationReportDto>> GetRenovationsReportAsync(int requesterId, int? gymId);
}
