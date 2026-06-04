using System.Security.Claims;
using GymAdmin.Application.DTOs.Memberships;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipsController(IMembershipService membershipService) => _membershipService = membershipService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<List<MembershipListDto>>> GetAll(
        [FromQuery] int? gymId,
        [FromQuery] int? alumnoId,
        [FromQuery] string? estado,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool? sortDesc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] DateTime? fechaVencimientoDesde,
        [FromQuery] DateTime? fechaVencimientoHasta,
        [FromQuery] bool? sinActiva)
    {
        var result = await _membershipService.GetAllAsync(
            GetUserId(),
            gymId,
            alumnoId,
            estado,
            search,
            sortBy,
            sortDesc,
            page,
            pageSize,
            fechaVencimientoDesde,
            fechaVencimientoHasta,
            sinActiva);
        AddPaginationHeaders(result.TotalCount, result.Page, result.PageSize);
        return Ok(result.Items);
    }

    [HttpGet("dashboard-summary")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<DashboardMembershipSummaryDto>> GetDashboardSummary([FromQuery] int? gymId)
    {
        var result = await _membershipService.GetDashboardSummaryAsync(GetUserId(), gymId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Alumno,Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<MembershipDto>> GetById(int id)
    {
        var membership = await _membershipService.GetByIdAsync(GetUserId(), id);
        return membership is null ? NotFound() : Ok(membership);
    }

    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<List<MembershipListDto>>> GetByStudent(int studentId) =>
        Ok(await _membershipService.GetByStudentIdAsync(GetUserId(), studentId));

    [HttpGet("student/{studentId}/access")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<StudentAccessDto>> GetStudentAccess(int studentId) =>
        Ok(await _membershipService.GetStudentAccessAsync(GetUserId(), studentId));

    [HttpGet("me/access")]
    [Authorize(Roles = "Alumno")]
    public async Task<ActionResult<StudentAccessDto>> GetMyAccess() =>
        Ok(await _membershipService.GetMyAccessAsync(GetUserId()));

    [HttpPost]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<MembershipDto>> Create([FromBody] CreateMembershipRequest request) =>
        Ok(await _membershipService.CreateAsync(GetUserId(), request));

    [HttpPost("student/{studentId}/renew")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<MembershipDto>> Renew(int studentId, [FromBody] RenewMembershipRequest request) =>
        Ok(await _membershipService.RenewAsync(GetUserId(), studentId, request));

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<MembershipDto>> Cancel(int id, [FromBody] CancelMembershipRequest request) =>
        Ok(await _membershipService.CancelAsync(GetUserId(), id, request));

    private void AddPaginationHeaders(int totalCount, int? page, int? pageSize)
    {
        Response.Headers["X-Total-Count"] = totalCount.ToString();
        if (page.HasValue)
            Response.Headers["X-Page"] = page.Value.ToString();
        if (pageSize.HasValue)
            Response.Headers["X-Page-Size"] = pageSize.Value.ToString();
    }
}
