using System.Security.Claims;
using GymAdmin.Application.DTOs.Assignments;
using GymAdmin.Application.DTOs.Routines;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet("my-routines")]
    [Authorize(Roles = "Alumno")]
    public async Task<ActionResult<List<RoutineDto>>> GetMyRoutines()
    {
        var routines = await _assignmentService.GetMyRoutinesAsync(GetUserId());
        return Ok(routines);
    }

    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<List<StudentRoutineDto>>> GetByStudent(int studentId)
    {
        var role = GetUserRole();
        int? profesorId = role == "Superusuario" ? null : GetUserId();
        var assignments = await _assignmentService.GetByStudentIdAsync(studentId, profesorId);
        return Ok(assignments);
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<AssignmentSummaryDto>> GetSummary()
    {
        var summary = await _assignmentService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpPost]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<StudentRoutineDto>> Assign([FromBody] AssignRoutineRequest request)
    {
        var role = GetUserRole();
        int? profesorId = role == "Superusuario" ? null : GetUserId();
        var assignment = await _assignmentService.AssignAsync(request, profesorId);
        return Ok(assignment);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<IActionResult> Unassign(int id)
    {
        var role = GetUserRole();
        int? profesorId = role == "Superusuario" ? null : GetUserId();
        await _assignmentService.UnassignAsync(id, profesorId);
        return NoContent();
    }
}
