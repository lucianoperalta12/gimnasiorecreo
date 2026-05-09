using GymAdmin.Application.DTOs.Assignments;
using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IAssignmentService
{
    Task<StudentRoutineDto> AssignAsync(AssignRoutineRequest request);
    Task UnassignAsync(int assignmentId);
    Task<List<StudentRoutineDto>> GetByStudentIdAsync(int studentId);
    Task<List<RoutineDto>> GetMyRoutinesAsync(int studentId);
    Task<AssignmentSummaryDto> GetSummaryAsync();
}
