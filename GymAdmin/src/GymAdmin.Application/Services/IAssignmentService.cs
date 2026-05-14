using GymAdmin.Application.DTOs.Assignments;
using GymAdmin.Application.DTOs.Routines;

namespace GymAdmin.Application.Services;

public interface IAssignmentService
{
    Task<StudentRoutineDto> AssignAsync(int requesterId, AssignRoutineRequest request);
    Task UnassignAsync(int requesterId, int assignmentId);
    Task<List<StudentRoutineDto>> GetByStudentIdAsync(int requesterId, int studentId);
    Task<List<RoutineDto>> GetMyRoutinesAsync(int studentId);
    Task<AssignmentSummaryDto> GetSummaryAsync(int requesterId);
}
