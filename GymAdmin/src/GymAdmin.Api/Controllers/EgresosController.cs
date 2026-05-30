using System.Security.Claims;
using GymAdmin.Application.DTOs.Egresos;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Superusuario,Administrativo")]
public class EgresosController : ControllerBase
{
    private readonly IEgresoService _egresoService;

    public EgresosController(IEgresoService egresoService) => _egresoService = egresoService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<EgresoListDto>>> GetAll(
        [FromQuery] int? gymId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        var result = await _egresoService.GetAllAsync(GetUserId(), gymId, page, pageSize);
        AddPaginationHeaders(result.TotalCount, result.Page, result.PageSize);
        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EgresoDto>> GetById(int id)
    {
        var egreso = await _egresoService.GetByIdAsync(GetUserId(), id);
        return egreso is null ? NotFound() : Ok(egreso);
    }

    [HttpPost]
    public async Task<ActionResult<EgresoDto>> Create([FromBody] CreateEgresoRequest request) =>
        Ok(await _egresoService.CreateAsync(GetUserId(), request));

    [HttpPut("{id}")]
    public async Task<ActionResult<EgresoDto>> Update(int id, [FromBody] UpdateEgresoRequest request) =>
        Ok(await _egresoService.UpdateAsync(GetUserId(), id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _egresoService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }

    private void AddPaginationHeaders(int totalCount, int? page, int? pageSize)
    {
        Response.Headers["X-Total-Count"] = totalCount.ToString();
        if (page.HasValue) Response.Headers["X-Page"] = page.Value.ToString();
        if (pageSize.HasValue) Response.Headers["X-Page-Size"] = pageSize.Value.ToString();
    }
}
