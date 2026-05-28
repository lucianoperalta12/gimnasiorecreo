using System.Security.Claims;
using GymAdmin.Application.DTOs.Auth;
using GymAdmin.Application.DTOs.Users;
using GymAdmin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymAdmin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponse>> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        var result = await _authService.LoginWithGoogleAsync(request.Credential);
        return Ok(result);
    }

    [HttpPost("login-admin")]
    public async Task<ActionResult<AuthResponse>> AdminLogin([FromBody] AuthAdminRequest request)
    {
        var result = await _authService.LoginAdminAsync(request.Username, request.Password);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try {
            var result = await _authService.RegisterAsync(request.Nombre, request.Email, request.Password);
            return Ok(result);
        } catch (InvalidOperationException ex) {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] TokenRefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("select-gym")]
    public async Task<ActionResult<AuthResponse>> SelectGym([FromBody] SelectGymRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.SelectGymAsync(userId, request.GymId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.LogoutAsync(userId);
        return NoContent();
    }
}
