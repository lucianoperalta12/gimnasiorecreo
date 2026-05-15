using System.Security.Claims;
using GymAdmin.Application.DTOs.MembershipPlans;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Superusuario,Administrativo")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _planService;

    public MembershipPlansController(IMembershipPlanService planService) => _planService = planService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<MembershipPlanDto>>> GetAll([FromQuery] int? gymId) =>
        Ok(await _planService.GetAllAsync(GetUserId(), gymId));

    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPlanDto>> GetById(int id)
    {
        var plan = await _planService.GetByIdAsync(GetUserId(), id);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<MembershipPlanDto>> Create([FromBody] CreateMembershipPlanRequest request) =>
        Ok(await _planService.CreateAsync(GetUserId(), request));

    [HttpPut("{id}")]
    public async Task<ActionResult<MembershipPlanDto>> Update(int id, [FromBody] UpdateMembershipPlanRequest request) =>
        Ok(await _planService.UpdateAsync(GetUserId(), id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _planService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }
}
