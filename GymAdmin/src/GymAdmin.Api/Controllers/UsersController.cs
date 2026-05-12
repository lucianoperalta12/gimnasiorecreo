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

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Superusuario")]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Superusuario")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpGet("students")]
    [Authorize(Roles = "Profesor,Superusuario")]
    public async Task<ActionResult<List<UserDto>>> GetStudents()
    {
        var students = await _userService.GetStudentsAsync();
        return Ok(students);
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Superusuario")]
    public async Task<ActionResult<UserDto>> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var user = await _userService.ChangeRoleAsync(id, request);
        return Ok(user);
    }

    [HttpPut("{id}/password")]
    [Authorize(Roles = "Superusuario")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        await _userService.ChangePasswordAsync(id, request);
        return Ok(new { message = "Contraseña actualizada exitosamente" });
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "Superusuario")]
    public async Task<ActionResult<UserDto>> ToggleStatus(int id)
    {
        var user = await _userService.ToggleStatusAsync(id);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superusuario")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
