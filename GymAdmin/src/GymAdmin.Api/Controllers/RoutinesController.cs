using System.Security.Claims;
using GymAdmin.Application.DTOs.Routines;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutinesController : ControllerBase
{
    private readonly IRoutineService _routineService;

    public RoutinesController(IRoutineService routineService)
    {
        _routineService = routineService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetUserRole() => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<List<RoutineListDto>>> GetAll()
    {
        var routines = await _routineService.GetAllAsync();
        return Ok(routines);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoutineDto>> GetById(int id)
    {
        var role = GetUserRole();
        int? studentId = role == "Alumno" ? GetUserId() : null;
        var routine = await _routineService.GetByIdAsync(id, studentId);
        if (routine is null) return NotFound();
        return Ok(routine);
    }

    [HttpPost]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<RoutineDto>> Create([FromBody] CreateRoutineRequest request)
    {
        var routine = await _routineService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, routine);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<RoutineDto>> Update(int id, [FromBody] UpdateRoutineRequest request)
    {
        var routine = await _routineService.UpdateAsync(id, request);
        return Ok(routine);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<IActionResult> Delete(int id)
    {
        await _routineService.DeleteAsync(id);
        return NoContent();
    }
}
