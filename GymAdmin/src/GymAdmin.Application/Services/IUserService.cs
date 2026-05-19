using GymAdmin.Application.DTOs.Users;

namespace GymAdmin.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync(int requesterId);
    Task<UserDto?> GetUserByIdAsync(int requesterId, int id);
    Task<List<UserDto>> GetStudentsAsync(int requesterId);
    Task<UserDto> CreateUserAsync(int requesterId, CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(int requesterId, int userId, UpdateUserRequest request);
    Task<UserDto> ChangeRoleAsync(int requesterId, int userId, ChangeRoleRequest request);
    Task ChangePasswordAsync(int requesterId, int userId, ChangePasswordRequest request);
    Task ChangeMyInitialPasswordAsync(int requesterId, ChangePasswordRequest request);
    Task<UserDto> ToggleStatusAsync(int requesterId, int userId);
    Task DeleteUserAsync(int requesterId, int userId);
}
