using GymAdmin.Application.DTOs.Users;

namespace GymAdmin.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<List<UserDto>> GetStudentsAsync();
    Task<UserDto> ChangeRoleAsync(int userId, ChangeRoleRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserDto> ToggleStatusAsync(int userId);
    Task DeleteUserAsync(int userId);
}
