using GymAdmin.Application.DTOs.Exercises;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Profesor,Superusuario")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExerciseDto>>> GetAll()
    {
        var exercises = await _exerciseService.GetAllAsync();
        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetById(int id)
    {
        var exercise = await _exerciseService.GetByIdAsync(id);
        if (exercise is null) return NotFound();
        return Ok(exercise);
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseDto>> Create([FromBody] CreateExerciseRequest request)
    {
        var exercise = await _exerciseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, exercise);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExerciseDto>> Update(int id, [FromBody] UpdateExerciseRequest request)
    {
        var exercise = await _exerciseService.UpdateAsync(id, request);
        return Ok(exercise);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _exerciseService.DeleteAsync(id);
        return NoContent();
    }
}
