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

    [HttpGet]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<List<RoutineListDto>>> GetAll() =>
        Ok(await _routineService.GetAllAsync(GetUserId()));

    [HttpGet("{id}")]
    public async Task<ActionResult<RoutineDto>> GetById(int id)
    {
        var routine = await _routineService.GetByIdAsync(GetUserId(), id);
        return routine is null ? NotFound() : Ok(routine);
    }

    [HttpPost]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<RoutineDto>> Create([FromBody] CreateRoutineRequest request)
    {
        var routine = await _routineService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, routine);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<RoutineDto>> Update(int id, [FromBody] UpdateRoutineRequest request) =>
        Ok(await _routineService.UpdateAsync(GetUserId(), id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<IActionResult> Delete(int id)
    {
        await _routineService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }
}
