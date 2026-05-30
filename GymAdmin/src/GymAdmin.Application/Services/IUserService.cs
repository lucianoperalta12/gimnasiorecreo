using GymAdmin.Application.DTOs.Common;
using GymAdmin.Application.DTOs.Users;

namespace GymAdmin.Application.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllUsersAsync(int requesterId, int? page = null, int? pageSize = null);
    Task<UserDto?> GetUserByIdAsync(int requesterId, int id, int? gymId = null);
    Task<PagedResult<UserDto>> GetStudentsAsync(int requesterId, int? page = null, int? pageSize = null);
    Task<UserDto> CreateUserAsync(int requesterId, CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(int requesterId, int userId, UpdateUserRequest request);
    Task<UserDto> ChangeRoleAsync(int requesterId, int userId, ChangeRoleRequest request);
    Task ChangePasswordAsync(int requesterId, int userId, ChangePasswordRequest request);
    Task ChangeMyInitialPasswordAsync(int requesterId, ChangePasswordRequest request);
    Task<UserDto> ToggleStatusAsync(int requesterId, int userId, int? gymId = null);
    Task DeleteUserAsync(int requesterId, int userId, int? gymId = null);
}
