using System.Security.Claims;
using GymAdmin.Application.DTOs.Auth;
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

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] TokenRefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
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
