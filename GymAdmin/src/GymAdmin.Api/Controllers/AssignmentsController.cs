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

    [HttpGet("my-routines")]
    [Authorize(Roles = "Alumno,Administrativo")]
    public async Task<ActionResult<List<RoutineDto>>> GetMyRoutines() =>
        Ok(await _assignmentService.GetMyRoutinesAsync(GetUserId()));

    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<List<StudentRoutineDto>>> GetByStudent(int studentId) =>
        Ok(await _assignmentService.GetByStudentIdAsync(GetUserId(), studentId));

    [HttpGet("summary")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<AssignmentSummaryDto>> GetSummary() =>
        Ok(await _assignmentService.GetSummaryAsync(GetUserId()));

    [HttpPost]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<StudentRoutineDto>> Assign([FromBody] AssignRoutineRequest request) =>
        Ok(await _assignmentService.AssignAsync(GetUserId(), request));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<IActionResult> Unassign(int id)
    {
        await _assignmentService.UnassignAsync(GetUserId(), id);
        return NoContent();
    }
}
