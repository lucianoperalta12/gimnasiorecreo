using GymAdmin.Application.DTOs.Gyms;

namespace GymAdmin.Application.Services;

public interface IGymService
{
    Task<List<GymDto>> GetAllAsync();
    Task<GymDto?> GetByIdAsync(int id);
    Task<GymDto> CreateAsync(CreateGymRequest request);
    Task<GymDto> UpdateAsync(int id, UpdateGymRequest request);
    Task<GymDto> ToggleStatusAsync(int id);
}
