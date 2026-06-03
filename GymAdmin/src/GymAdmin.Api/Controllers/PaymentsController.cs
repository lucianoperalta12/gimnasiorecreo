using System.Security.Claims;
using GymAdmin.Application.DTOs.Payments;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Superusuario,Administrativo")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<PaymentListDto>>> GetAll(
      [FromQuery] int? gymId,
      [FromQuery] int? membresiaId,
      [FromQuery] DateOnly? fechaDesde,
      [FromQuery] DateOnly? fechaHasta,
      [FromQuery] int? page,
      [FromQuery] int? pageSize)
    {
        var result = await _paymentService.GetAllAsync(
            GetUserId(),
            gymId,
            membresiaId,
            fechaDesde,
            fechaHasta,
            page,
            pageSize);

        AddPaginationHeaders(result.TotalCount, result.Page, result.PageSize);

        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _paymentService.GetByIdAsync(GetUserId(), id);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpGet("membership/{membresiaId}")]
    public async Task<ActionResult<List<PaymentListDto>>> GetByMembership(int membresiaId) =>
        Ok(await _paymentService.GetByMembershipIdAsync(GetUserId(), membresiaId));

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentRequest request) =>
        Ok(await _paymentService.CreateAsync(GetUserId(), request));

    [HttpPut("{id}")]
    public async Task<ActionResult<PaymentDto>> Update(int id, [FromBody] UpdatePaymentRequest request) =>
        Ok(await _paymentService.UpdateAsync(GetUserId(), id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _paymentService.DeleteAsync(GetUserId(), id);
        return NoContent();
    }

    private void AddPaginationHeaders(int totalCount, int? page, int? pageSize)
    {
        Response.Headers["X-Total-Count"] = totalCount.ToString();
        if (page.HasValue)
            Response.Headers["X-Page"] = page.Value.ToString();
        if (pageSize.HasValue)
            Response.Headers["X-Page-Size"] = pageSize.Value.ToString();
    }
}
