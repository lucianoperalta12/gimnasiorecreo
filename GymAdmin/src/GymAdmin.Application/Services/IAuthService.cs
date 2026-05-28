using GymAdmin.Application.DTOs.Auth;

namespace GymAdmin.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginWithGoogleAsync(string credential);
    Task<AuthResponse> LoginAdminAsync(string username, string password);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<AuthResponse> RegisterAsync(string nombre, string email, string password);
    Task LogoutAsync(int userId);
    Task<AuthResponse> SelectGymAsync(int userId, int gymId);
}
