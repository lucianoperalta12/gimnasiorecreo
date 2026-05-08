using GymAdmin.Application.DTOs.Auth;

namespace GymAdmin.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginWithGoogleAsync(string credential);
    Task<AuthResponse> LoginAdminAsync(string username, string password);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(int userId);
}
