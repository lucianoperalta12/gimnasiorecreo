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
        [FromQuery] int? gymId) =>
        Ok(await _ingresoService.GetAllAsync(GetUserId(), fechaDesde, fechaHasta, alumnoId, gymId));

    [HttpPost("registrar")]
    [Authorize(Roles = "Terminal")]
    public async Task<ActionResult<RegistrarIngresoResponse>> Registrar([FromBody] RegistrarIngresoRequest request) =>
        Ok(await _ingresoService.RegistrarAsync(GetUserId(), request));
}
