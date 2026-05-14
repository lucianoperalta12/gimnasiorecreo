using System.Security.Claims;
using GymAdmin.Application.DTOs.Exercises;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Profesor,Superusuario,Administrativo")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;
    public ExercisesController(IExerciseService exerciseService) { _exerciseService = exerciseService; }
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ExerciseDto>>> GetAll() => Ok(await _exerciseService.GetAllAsync(GetUserId()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetById(int id)
    {
        var exercise = await _exerciseService.GetByIdAsync(GetUserId(), id);
        return exercise is null ? NotFound() : Ok(exercise);
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> Create([FromBody] CreateExerciseRequest request) => Ok(await _exerciseService.CreateAsync(GetUserId(), request));

    [HttpPut("{id}")]
    public async Task<ActionResult<ExerciseDto>> Update(int id, [FromBody] UpdateExerciseRequest request) => Ok(await _exerciseService.UpdateAsync(GetUserId(), id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _exerciseService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }
}
