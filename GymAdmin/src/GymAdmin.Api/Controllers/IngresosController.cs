using System.Security.Claims;
using GymAdmin.Application.DTOs.Ingresos;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngresosController : ControllerBase
{
    private readonly IIngresoService _ingresoService;

    public IngresosController(IIngresoService ingresoService) => _ingresoService = ingresoService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<List<IngresoListItemDto>>> GetAll(
        [FromQuery] DateOnly? fechaDesde, 
        [FromQuery] DateOnly? fechaHasta, 
        [FromQuery] int? alumnoId, 
        [FromQuery] int? gymId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _ingresoService.GetAllAsync(GetUserId(), fechaDesde, fechaHasta, alumnoId, gymId, page, pageSize);
        AddPaginationHeaders(result.TotalCount, result.Page, result.PageSize);
        return Ok(result.Items);
    }

    [HttpGet("today")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<List<IngresoHoyItemDto>>> GetToday([FromQuery] int? gymId) =>
        Ok(await _ingresoService.GetTodayAsync(GetUserId(), gymId));

    [HttpPost("registrar")]
    [Authorize(Roles = "Terminal,Administrativo,Superusuario")]
    public async Task<ActionResult<RegistrarIngresoResponse>> Registrar([FromBody] RegistrarIngresoRequest request) =>
        Ok(await _ingresoService.RegistrarAsync(GetUserId(), request));

    private void AddPaginationHeaders(int totalCount, int? page, int? pageSize)
    {
        Response.Headers["X-Total-Count"] = totalCount.ToString();
        if (page.HasValue)
            Response.Headers["X-Page"] = page.Value.ToString();
        if (pageSize.HasValue)
            Response.Headers["X-Page-Size"] = pageSize.Value.ToString();
    }
}
