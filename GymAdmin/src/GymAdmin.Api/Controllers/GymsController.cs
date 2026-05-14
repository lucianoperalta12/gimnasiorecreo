using GymAdmin.Application.DTOs.Gyms;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Superusuario")]
public class GymsController : ControllerBase
{
    private readonly IGymService _gymService;
    public GymsController(IGymService gymService) { _gymService = gymService; }

    [HttpGet]
    public async Task<ActionResult<List<GymDto>>> GetAll() => Ok(await _gymService.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<GymDto>> Create([FromBody] CreateGymRequest request) => Ok(await _gymService.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<ActionResult<GymDto>> Update(int id, [FromBody] UpdateGymRequest request) => Ok(await _gymService.UpdateAsync(id, request));

    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<GymDto>> ToggleStatus(int id) => Ok(await _gymService.ToggleStatusAsync(id));
}
