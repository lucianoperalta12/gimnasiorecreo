using System.Security.Claims;
using GymAdmin.Application.DTOs.Users;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) { _userService = userService; }
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<List<UserDto>>> GetAll() => Ok(await _userService.GetAllUsersAsync(GetUserId()));

    [HttpGet("{id}")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(GetUserId(), id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("students")]
    [Authorize(Roles = "Profesor,Superusuario,Administrativo")]
    public async Task<ActionResult<List<UserDto>>> GetStudents() => Ok(await _userService.GetStudentsAsync(GetUserId()));

    [HttpPost]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request) => Ok(await _userService.CreateUserAsync(GetUserId(), request));

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<UserDto>> ChangeRole(int id, [FromBody] ChangeRoleRequest request) => Ok(await _userService.ChangeRoleAsync(GetUserId(), id, request));

    [HttpPut("{id}/password")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        await _userService.ChangePasswordAsync(GetUserId(), id, request);
        return Ok(new { message = "Contraseña actualizada exitosamente" });
    }

    [HttpPut("me/change-initial-password")]
    public async Task<IActionResult> ChangeInitialPassword([FromBody] ChangePasswordRequest request)
    {
        await _userService.ChangeMyInitialPasswordAsync(GetUserId(), request);
        return Ok(new { message = "Contraseña actualizada exitosamente" });
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<ActionResult<UserDto>> ToggleStatus(int id) => Ok(await _userService.ToggleStatusAsync(GetUserId(), id));

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superusuario,Administrativo")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(GetUserId(), id);
        return NoContent();
    }
}
