using GymAdmin.Application.DTOs.Assignments;
using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IAssignmentService
{
    Task<StudentRoutineDto> AssignAsync(AssignRoutineRequest request, int? profesorId = null);
    Task UnassignAsync(int assignmentId, int? profesorId = null);
    Task<List<StudentRoutineDto>> GetByStudentIdAsync(int studentId, int? profesorId = null);
    Task<List<RoutineDto>> GetMyRoutinesAsync(int studentId);
    Task<AssignmentSummaryDto> GetSummaryAsync();
}
