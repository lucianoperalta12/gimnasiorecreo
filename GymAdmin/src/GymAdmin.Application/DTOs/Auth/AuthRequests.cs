using GymAdmin.Application.DTOs.Users;

namespace GymAdmin.Application.DTOs.Auth;

public record GoogleAuthRequest(string Credential);
public record AuthAdminRequest(string Username, string Password);
public record TokenRefreshRequest(string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
